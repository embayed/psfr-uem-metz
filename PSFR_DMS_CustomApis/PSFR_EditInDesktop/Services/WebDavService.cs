using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PSFR_EditInDesktop.Constants;
using PSFR_EditInDesktop.Helpers;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PSFR_EditInDesktop.Services;

public class WebDavService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WebDavService> logger, WebDavStateHelper stateService, IHttpContextAccessor httpContextAccessor) : IWebDavService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("default");
    private readonly ILogger<WebDavService> _logger = logger;
    private readonly string _DMSApiUrl = configuration[WebDavConstants.ExternalApiUrlConfigKey]
        ?? throw new Exception("DMS URL Not Configured.");

    private readonly WebDavStateHelper _stateService = stateService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<IActionResult> GetEditUrlAsync(string documentId, string version, bool fromAttachment, string token, string mode)
    {
        _logger.LogInformation(WebDavConstants.LogMessages.GetEditUrlCalled, documentId, version, fromAttachment, !string.IsNullOrEmpty(token), mode);

        try
        {
            string decodedToken = string.Empty;
            if (!string.IsNullOrEmpty(token))
            {
                decodedToken = Uri.UnescapeDataString(token);
            }
            else
            {
                _logger.LogWarning(WebDavConstants.LogMessages.NoTokenProvidedGetEditUrl, documentId);
            }

            string latestVersion = await GetLatestVersionAsync(documentId, version, decodedToken);

            string apiUrl = string.Format(WebDavConstants.UrlTemplates.ViewerFileVersion, _DMSApiUrl, documentId, latestVersion, fromAttachment);

            HttpRequestMessage request = new(HttpMethod.Get, apiUrl);
            if (!string.IsNullOrEmpty(decodedToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", decodedToken);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            // Log response details for debugging
            _logger.LogInformation("Viewer API response - Status: {StatusCode}, Content-Type: {ContentType}",
                response.StatusCode, response.Content.Headers.ContentType?.MediaType ?? "null");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(WebDavConstants.LogMessages.ViewerApiReturnedStatusForDocument, response.StatusCode, documentId);
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Viewer API error response: {ErrorContent}", errorContent);
                return new ObjectResult($"Viewer API returned {response.StatusCode}: {errorContent}") { StatusCode = (int)response.StatusCode };
            }

            // Check if we got JSON, not HTML
            var contentType = response.Content.Headers.ContentType?.MediaType;

            // Read content once for all checks
            string json = await response.Content.ReadAsStringAsync();

            // Check if content starts with HTML tags (more reliable than content-type header)
            if (json.TrimStart().StartsWith("<") ||
                contentType?.Contains("text/html") == true ||
                contentType?.Contains("text/xml") == true)
            {
                _logger.LogError("Viewer API returned HTML/XML instead of JSON. Content-Type: {ContentType}, Response: {Content}",
                    contentType ?? "null", json.Substring(0, Math.Min(1000, json.Length)));
                return new ObjectResult($"Viewer API returned HTML/XML instead of JSON. Content-Type: {contentType ?? "null"}. Check authentication and API endpoint.") { StatusCode = 500 };
            }

            _logger.LogInformation(WebDavConstants.LogMessages.ViewerApiResponse, json);

            JsonElement data;
            try
            {
                data = JsonSerializer.Deserialize<JsonElement>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JSON response from Viewer API. Response content: {Json}", json);
                return new ObjectResult($"Invalid JSON response from Viewer API: {ex.Message}") { StatusCode = 500 };
            }

            string fileJsonString = data.GetProperty("file").GetString() ?? string.Empty;
            _logger.LogInformation(WebDavConstants.LogMessages.FileJsonString, fileJsonString);

            if (string.IsNullOrEmpty(fileJsonString))
            {
                _logger.LogError(WebDavConstants.LogMessages.FileJsonStringEmpty, documentId);
                return new ObjectResult(WebDavConstants.Messages.ViewerEmptyFileData) { StatusCode = 500 };
            }

            JsonElement fileJson = JsonSerializer.Deserialize<JsonElement>(fileJsonString);

            string fileName = fileJson.GetProperty("fileName").GetString() ?? "file.mpp";
            _logger.LogInformation(WebDavConstants.LogMessages.ExtractedFileName, fileName);

            string ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (!string.IsNullOrEmpty(decodedToken))
            {
                WebDavStateHelper.SetToken(documentId, decodedToken);
            }
            else
            {
                _logger.LogWarning(WebDavConstants.LogMessages.NoTokenToStoreForDocument, documentId);
            }

            HttpContext httpContext = GetHttpContext();
            string webDavUrl = string.Format(WebDavConstants.UrlTemplates.WebDavFileUrl, httpContext.Request.Scheme, httpContext.Request.Host.ToString(), documentId, Uri.EscapeDataString(fileName));

            string lowerMode = mode.ToLowerInvariant();
            string protocol = lowerMode == WebDavConstants.ModeView ? WebDavConstants.OfficeProtocolView : WebDavConstants.OfficeProtocolEdit;

            // For AutoCAD, add query parameters with checkout/checkin info
            string autocadUrl = webDavUrl;
            if (ext == ".dwg" || ext == ".dxf" || ext == ".dwt")
            {
                // Pass the Viewer API URL (not the WebDAV API URL)
                autocadUrl = $"{webDavUrl}?documentId={documentId}&token={Uri.EscapeDataString(decodedToken)}&version={latestVersion}&fromAttachment={fromAttachment}&viewerApiUrl={Uri.EscapeDataString(_DMSApiUrl)}";
            }

            string officeUri = ext switch
            {
                ".mpp" => "ms-project:" + protocol + "|u|" + webDavUrl,
                ".vsdx" => "ms-visio:" + protocol + "|u|" + webDavUrl,
                ".vsd" => "ms-visio:" + protocol + "|u|" + webDavUrl,
                ".docx" => "ms-word:" + protocol + "|u|" + webDavUrl,
                ".doc" => "ms-word:" + protocol + "|u|" + webDavUrl,
                ".xlsx" => "ms-excel:" + protocol + "|u|" + webDavUrl,
                ".xls" => "ms-excel:" + protocol + "|u|" + webDavUrl,
                ".pptx" => "ms-powerpoint:" + protocol + "|u|" + webDavUrl,
                ".ppt" => "ms-powerpoint:" + protocol + "|u|" + webDavUrl,
                ".dwg" => (lowerMode == WebDavConstants.ModeView ? "acad-view:" : "acad-edit:") + autocadUrl,
                ".dxf" => (lowerMode == WebDavConstants.ModeView ? "acad-view:" : "acad-edit:") + autocadUrl,
                ".dwt" => (lowerMode == WebDavConstants.ModeView ? "acad-view:" : "acad-edit:") + autocadUrl,
                _ => "ms-word:" + protocol + "|u|" + webDavUrl
            };

            _logger.LogInformation(WebDavConstants.LogMessages.GeneratedOfficeUri, officeUri, mode);

            bool isCheckedOut = false;
            WebDavSessionInfo? existingSession = WebDavStateHelper.GetSession(documentId);
            if (existingSession != null)
            {
                isCheckedOut = existingSession.IsCheckedOut;
            }

            WebDavStateHelper.SetSession(documentId, new WebDavSessionInfo(fileName, latestVersion, fromAttachment, decodedToken, isCheckedOut, mode));

            return new OkObjectResult(new { webDavUrl, officeUri, fileName, mode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting edit URL for document {DocumentId}", documentId);
            return new ObjectResult("Error: " + ex.Message) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> GetFileAsync(string documentId, string fileName)
    {
        string? token = WebDavStateHelper.GetToken(documentId);
        string version = WebDavConstants.VersionCurrent;
        bool fromAttachment = false;

        WebDavSessionInfo? session = WebDavStateHelper.GetSession(documentId);
        if (session != null)
        {
            version = session.Version;
            fromAttachment = session.FromAttachment;
        }
        else if (token == null)
        {
            _logger.LogWarning(WebDavConstants.LogMessages.NoTokenInCache, documentId);
        }

        _logger.LogInformation(WebDavConstants.LogMessages.WebDavGetRequest, documentId, fileName, version, token != null);

        try
        {
            string apiUrl = string.Format(WebDavConstants.UrlTemplates.ViewerFileContent, _DMSApiUrl, documentId, version, fromAttachment);

            _logger.LogInformation(WebDavConstants.LogMessages.CallingViewerApiUrl, apiUrl);

            HttpRequestMessage request = new(HttpMethod.Get, apiUrl);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning(WebDavConstants.LogMessages.NoAuthTokenForViewerRequest);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.Content.Headers.ContentType != null && response.Content.Headers.ContentType.MediaType == WebDavConstants.ContentTypeTextHtml)
            {
                _logger.LogError("Viewer API returned HTML instead of file content - authentication likely failed");
                return new ObjectResult(WebDavConstants.Messages.AuthFailedLoginPage) { StatusCode = 401 };
            }

            _logger.LogInformation(WebDavConstants.LogMessages.ViewerApiStatus, response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(WebDavConstants.LogMessages.ViewerApiReturnedStatusForDocument, response.StatusCode, documentId);
                return new NotFoundObjectResult(string.Format(WebDavConstants.Messages.FileNotFoundFormat, documentId));
            }

            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            string contentType = ExtensionsHelper.GetMsExtContentType(fileName);

            _logger.LogInformation(WebDavConstants.LogMessages.ServingFileFromViewer, documentId, fileBytes.Length, contentType);

            if (fileBytes.Length > 0 && fileBytes.Length < 1000)
            {
                string preview = System.Text.Encoding.UTF8.GetString([.. fileBytes.Take(Math.Min(500, fileBytes.Length))]);
                _logger.LogWarning(WebDavConstants.LogMessages.FileContentPreview, preview);
            }

            if (fileBytes.Length > 0 && (fileBytes[0] == '{' || fileBytes[0] == '<' || fileBytes[0] == '['))
            {
                string textContent = System.Text.Encoding.UTF8.GetString(fileBytes);
                _logger.LogError(WebDavConstants.LogMessages.ReceivedTextInsteadOfBinary, textContent);
                return new ObjectResult(WebDavConstants.Messages.ServerReturnedTextInsteadOfBinary) { StatusCode = 500 };
            }

            HttpContext httpContext = GetHttpContext();
            httpContext.Response.Headers[WebDavConstants.HeaderAcceptRanges] = WebDavConstants.AcceptRangesBytes;
            httpContext.Response.Headers[WebDavConstants.HeaderContentDisposition] = "attachment; filename=\"" + fileName + "\"";

            FileContentResult fileResult = new(fileBytes, contentType)
            {
                FileDownloadName = fileName
            };
            return fileResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, WebDavConstants.LogMessages.ErrorFetchingFile, documentId);
            return new ObjectResult("Error fetching file: " + ex.Message) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> PutFileAsync(string documentId, string fileName)
    {
        string? token = WebDavStateHelper.GetToken(documentId);
        string version = WebDavConstants.VersionCurrent;
        bool fromAttachment = false;

        WebDavSessionInfo? session = WebDavStateHelper.GetSession(documentId);
        if (session != null)
        {
            version = session.Version;
            fromAttachment = session.FromAttachment;
        }

        _logger.LogInformation(WebDavConstants.LogMessages.WebDavPutRequest, documentId, fileName, version, token != null);

        if (token == null)
        {
            _logger.LogWarning(WebDavConstants.LogMessages.NoTokenForDocument, documentId);
        }

        try
        {
            HttpContext httpContext = GetHttpContext();

            // Get or create temp file path for this document
            string tempFilePath = GetTempFilePath(documentId, fileName);

            // Ensure temp directory exists
            string? tempDir = Path.GetDirectoryName(tempFilePath);
            if (!string.IsNullOrEmpty(tempDir) && !Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
                _logger.LogInformation("Created temp directory: {TempDir}", tempDir);
            }

            // Save to temp file
            using (FileStream fs = new(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await httpContext.Request.Body.CopyToAsync(fs);
            }

            long fileSize = new FileInfo(tempFilePath).Length;
            _logger.LogInformation("Saved to temp file: {TempFilePath} ({Size} bytes)", tempFilePath, fileSize);

            // Store temp file path in session
            WebDavSessionInfo? existingSession = WebDavStateHelper.GetSession(documentId);
            if (existingSession != null)
            {
                WebDavStateHelper.SetSession(documentId, existingSession with { TempFilePath = tempFilePath });
            }
            else
            {
                // Create new session with temp file path
                WebDavStateHelper.SetSession(documentId, new WebDavSessionInfo(fileName, version, fromAttachment, token ?? string.Empty, false, WebDavConstants.ModeEdit, tempFilePath));
            }

            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file to temp location for {DocumentId}", documentId);
            return new ObjectResult("Error saving file: " + ex.Message) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> LockAsync(string documentId, string fileName)
    {
        string? token = WebDavStateHelper.GetToken(documentId);
        string version = WebDavConstants.VersionCurrent;
        bool fromAttachment = false;
        string mode = WebDavConstants.ModeEdit;

        WebDavSessionInfo? session = WebDavStateHelper.GetSession(documentId);
        if (session != null)
        {
            version = session.Version;
            fromAttachment = session.FromAttachment;
            mode = session.Mode;
        }

        _logger.LogInformation(WebDavConstants.LogMessages.WebDavLockRequest, documentId, fileName, version, mode);

        try
        {
            bool checkoutSucceeded = false;
            string lowerMode = mode.ToLowerInvariant();

            if (lowerMode == WebDavConstants.ModeEdit)
            {
                string checkoutUrl = string.Format(WebDavConstants.UrlTemplates.ViewerFileCheckout, _DMSApiUrl, documentId, version, fromAttachment);

                HttpRequestMessage request = new(HttpMethod.Get, checkoutUrl);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _logger.LogWarning(WebDavConstants.LogMessages.NoTokenForLockRequest);
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                checkoutSucceeded = response.IsSuccessStatusCode;

                if (!checkoutSucceeded)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(WebDavConstants.LogMessages.FailedToCheckoutDocument, documentId, response.StatusCode, errorContent);
                }
                else
                {
                    _logger.LogInformation(WebDavConstants.LogMessages.CheckoutSucceeded, documentId);

                    WebDavSessionInfo? existingSession = WebDavStateHelper.GetSession(documentId);
                    if (existingSession != null)
                    {
                        WebDavStateHelper.SetSession(documentId, existingSession with { IsCheckedOut = true });
                    }
                }
            }
            else
            {
                _logger.LogInformation(WebDavConstants.LogMessages.SkippingCheckoutViewMode, documentId);
            }

            if (lowerMode == WebDavConstants.ModeView)
            {
                _logger.LogInformation(WebDavConstants.LogMessages.ViewModeReturningLocked, documentId);
                return new ObjectResult(WebDavConstants.Messages.ViewModeLocked) { StatusCode = 423 };
            }

            string lockToken = WebDavConstants.LockTokenPrefix + Guid.NewGuid();

            string xmlResponse = string.Format(WebDavConstants.XmlTemplates.LockResponse, WebDavConstants.LockDepth, WebDavConstants.LockOwner, WebDavConstants.LockTimeout, lockToken);

            HttpContext httpContext = GetHttpContext();
            httpContext.Response.Headers[WebDavConstants.HeaderLockToken] = "<" + lockToken + ">";
            _logger.LogInformation(WebDavConstants.LogMessages.FileLockedSuccessfully, documentId, lockToken);

            ContentResult contentResult = new()
            {
                Content = xmlResponse,
                ContentType = WebDavConstants.ContentTypeApplicationXmlUtf8,
                StatusCode = 200
            };
            return contentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, WebDavConstants.LogMessages.ErrorLockingFile, documentId);
            return new ObjectResult("Error locking file: " + ex.Message) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> UnlockAsync(string documentId, string fileName)
    {
        string? token = WebDavStateHelper.GetToken(documentId);
        string version = WebDavConstants.VersionCurrent;
        bool fromAttachment = false;
        string mode = WebDavConstants.ModeEdit;
        string? tempFilePath = null;

        WebDavSessionInfo? session = WebDavStateHelper.GetSession(documentId);
        if (session != null)
        {
            version = session.Version;
            fromAttachment = session.FromAttachment;
            mode = session.Mode;
            tempFilePath = session.TempFilePath;
        }

        _logger.LogInformation(WebDavConstants.LogMessages.WebDavUnlockRequest, documentId, fileName, version, mode);

        bool isCheckedOut = false;
        WebDavSessionInfo? existingSession = WebDavStateHelper.GetSession(documentId);
        if (existingSession != null)
        {
            isCheckedOut = existingSession.IsCheckedOut;
        }

        try
        {
            string lowerMode = mode.ToLowerInvariant();

            // Upload temp file to DMS if it exists
            if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
            {
                _logger.LogInformation("Uploading temp file to DMS: {TempFilePath}", tempFilePath);

                byte[] fileBytes = await File.ReadAllBytesAsync(tempFilePath);

                string checkinUrl = string.Format(
                    WebDavConstants.UrlTemplates.ViewerFileCheckin,
                    _DMSApiUrl, documentId, version,
                    WebDavConstants.QueryIsVersionModifiedTrue, // Modified version
                    fromAttachment);

                HttpRequestMessage request = new(HttpMethod.Post, checkinUrl);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                ByteArrayContent content = new(fileBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue(WebDavConstants.ContentTypeApplicationOctetStream);
                request.Content = content;

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to upload to DMS: {StatusCode}, Error: {ErrorContent}", response.StatusCode, errorContent);

                    // Don't delete temp file if upload failed - user can retry
                    return new ObjectResult($"Failed to save to DMS: {response.StatusCode}") { StatusCode = 500 };
                }

                _logger.LogInformation("Successfully uploaded to DMS for {DocumentId} ({Size} bytes)", documentId, fileBytes.Length);

                // Update version from response
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseContent))
                {
                    try
                    {
                        JsonElement jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        if (jsonResponse.TryGetProperty("version", out JsonElement versionProp))
                        {
                            string newVersion = versionProp.GetString() ?? version;
                            _logger.LogInformation("New version after checkin: {NewVersion}", newVersion);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not parse checkin response");
                    }
                }

                // Delete temp file after successful upload
                try
                {
                    File.Delete(tempFilePath);
                    _logger.LogInformation("Deleted temp file: {TempFilePath}", tempFilePath);

                    // Try to delete parent directory if empty
                    string? tempDir = Path.GetDirectoryName(tempFilePath);
                    if (!string.IsNullOrEmpty(tempDir) && Directory.Exists(tempDir))
                    {
                        if (!Directory.EnumerateFileSystemEntries(tempDir).Any())
                        {
                            Directory.Delete(tempDir);
                            _logger.LogInformation("Deleted empty temp directory: {TempDir}", tempDir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete temp file: {TempFilePath}", tempFilePath);
                    // Continue anyway - file will be cleaned up later by cleanup service
                }
            }
            else if (isCheckedOut && lowerMode == WebDavConstants.ModeEdit)
            {
                // No temp file but checked out - do regular checkin without modification
                _logger.LogInformation("No temp file found, performing checkin without modification for {DocumentId}", documentId);

                string checkinUrl = string.Format(
                    WebDavConstants.UrlTemplates.ViewerFileCheckin,
                    _DMSApiUrl, documentId, version,
                    WebDavConstants.QueryIsVersionModifiedFalse, // Not modified
                    fromAttachment);

                HttpRequestMessage request = new(HttpMethod.Post, checkinUrl);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(WebDavConstants.LogMessages.FailedToUnlockDocument, documentId, response.StatusCode);
                }
            }
            else
            {
                if (lowerMode == WebDavConstants.ModeView)
                {
                    _logger.LogInformation(WebDavConstants.LogMessages.ViewModeSkippingCheckinUnlock, documentId);
                }
                else
                {
                    _logger.LogInformation(WebDavConstants.LogMessages.FileAlreadyCheckedInUnlock, documentId);
                }
            }

            // Clean up session
            WebDavStateHelper.RemoveSession(documentId);
            WebDavStateHelper.RemoveToken(documentId);

            _logger.LogInformation(WebDavConstants.LogMessages.FileUnlockedSuccessfully, documentId);

            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, WebDavConstants.LogMessages.ErrorUnlockingFile, documentId);
            return new ObjectResult("Error unlocking file: " + ex.Message) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> HeadAsync(string documentId, string fileName)
    {
        string? token = WebDavStateHelper.GetToken(documentId);
        string version = WebDavConstants.VersionCurrent;
        bool fromAttachment = false;

        WebDavSessionInfo? session = WebDavStateHelper.GetSession(documentId);
        if (session != null)
        {
            version = session.Version;
            fromAttachment = session.FromAttachment;
        }

        try
        {
            string apiUrl = string.Format(WebDavConstants.UrlTemplates.ViewerFileContent, _DMSApiUrl, documentId, version, fromAttachment);

            HttpRequestMessage request = new(HttpMethod.Get, apiUrl);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new NotFoundResult();
            }

            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

            HttpContext httpContext = GetHttpContext();
            httpContext.Response.Headers[WebDavConstants.HeaderContentLength] = fileBytes.Length.ToString();
            httpContext.Response.Headers[WebDavConstants.HeaderContentType] = ExtensionsHelper.GetMsExtContentType(fileName);
            httpContext.Response.Headers[WebDavConstants.HeaderAcceptRanges] = WebDavConstants.AcceptRangesBytes;

            return new OkResult();
        }
        catch
        {
            return new NotFoundResult();
        }
    }

    public async Task<IActionResult> PropFindAsync(string documentId, string fileName)
    {
        string? token = WebDavStateHelper.GetToken(documentId);
        string version = WebDavConstants.VersionCurrent;
        bool fromAttachment = false;

        WebDavSessionInfo? session = WebDavStateHelper.GetSession(documentId);
        if (session != null)
        {
            version = session.Version;
            fromAttachment = session.FromAttachment;
        }

        try
        {
            string apiUrl = string.Format(WebDavConstants.UrlTemplates.ViewerFileContent, _DMSApiUrl, documentId, version, fromAttachment);

            HttpRequestMessage request = new(HttpMethod.Get, apiUrl);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new NotFoundResult();
            }

            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            string lastModified = DateTime.UtcNow.ToString("R");

            string xmlResponse = string.Format(WebDavConstants.XmlTemplates.PropFindResponse, WebDavConstants.WebDavRouteBase, documentId, Uri.EscapeDataString(fileName), fileBytes.Length, ExtensionsHelper.GetMsExtContentType(fileName), lastModified);

            ContentResult contentResult = new()
            {
                Content = xmlResponse,
                ContentType = WebDavConstants.ContentTypeApplicationXmlUtf8,
                StatusCode = 200
            };
            return contentResult;
        }
        catch
        {
            return new NotFoundResult();
        }
    }

    public IActionResult Options()
    {
        HttpContext httpContext = GetHttpContext();
        httpContext.Response.Headers[WebDavConstants.HeaderAllow] = WebDavConstants.HeaderAllowMethods;
        httpContext.Response.Headers[WebDavConstants.HeaderDav] = WebDavConstants.DavHeaderValue;
        httpContext.Response.Headers[WebDavConstants.HeaderMsAuthorVia] = WebDavConstants.MsAuthorViaValue;
        httpContext.Response.Headers[WebDavConstants.HeaderPublic] = WebDavConstants.HeaderAllowMethods;
        httpContext.Response.Headers[WebDavConstants.HeaderAccessControlAllowOrigin] = WebDavConstants.AccessControlAllowOriginAll;
        httpContext.Response.Headers[WebDavConstants.HeaderAccessControlAllowMethods] = WebDavConstants.HeaderAllowMethods;
        httpContext.Response.Headers[WebDavConstants.HeaderAccessControlAllowHeaders] = WebDavConstants.HeaderAccessControlAllowHeadersAll;
        return new OkResult();
    }

    private async Task<string> GetLatestVersionAsync(string documentId, string requestedVersion, string decodedToken)
    {
        string latestVersion = requestedVersion;

        try
        {
            string versionsUrl = string.Format(WebDavConstants.UrlTemplates.ViewerVersions, _DMSApiUrl, documentId);

            HttpRequestMessage versionsRequest = new(HttpMethod.Get, versionsUrl);
            if (!string.IsNullOrEmpty(decodedToken))
            {
                versionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", decodedToken);
            }

            HttpResponseMessage versionsResponse = await _httpClient.SendAsync(versionsRequest);
            if (versionsResponse.IsSuccessStatusCode)
            {
                // Check if we got JSON, not HTML
                var contentType = versionsResponse.Content.Headers.ContentType?.MediaType;
                if (contentType?.Contains("text/html") == true || contentType?.Contains("text/xml") == true)
                {
                    string htmlContent = await versionsResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Versions API returned HTML/XML instead of JSON. Content-Type: {ContentType}. Falling back to requested version.", contentType);
                    return requestedVersion;
                }

                string versionsJson = await versionsResponse.Content.ReadAsStringAsync();

                JsonElement versionsList;
                try
                {
                    versionsList = JsonSerializer.Deserialize<JsonElement>(versionsJson);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse versions JSON. Falling back to requested version {RequestedVersion}. Response: {VersionsJson}", requestedVersion, versionsJson);
                    return requestedVersion;
                }

                if (versionsList.ValueKind == JsonValueKind.Array && versionsList.GetArrayLength() > 0)
                {
                    double maxVersion = 0;

                    foreach (JsonElement versionObj in versionsList.EnumerateArray())
                    {
                        if (versionObj.TryGetProperty("VersionNumber", out JsonElement versionNum))
                        {
                            double vNum = 0;

                            if (versionNum.ValueKind == JsonValueKind.String)
                            {
                                string? versionStr = versionNum.GetString();
                                if (double.TryParse(versionStr, out vNum) && vNum > maxVersion)
                                {
                                    maxVersion = vNum;
                                }
                            }
                            else if (versionNum.TryGetDouble(out vNum) && vNum > maxVersion)
                            {
                                maxVersion = vNum;
                            }
                        }
                    }

                    if (maxVersion > 0)
                    {
                        latestVersion = maxVersion.ToString("0.0");
                        _logger.LogInformation(WebDavConstants.LogMessages.UsingLatestVersion, latestVersion, requestedVersion);
                    }
                }
            }
            else
            {
                _logger.LogWarning(WebDavConstants.LogMessages.FailedToFetchVersionsFallback, versionsResponse.StatusCode, requestedVersion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, WebDavConstants.LogMessages.ErrorFetchingVersionsFallback, requestedVersion);
        }

        return latestVersion;
    }

    private string GetTempFilePath(string documentId, string fileName)
    {
        // Create temp directory structure: %TEMP%/WebDav/{documentId}/
        string tempBase = Path.Combine(Path.GetTempPath(), "WebDav", documentId);
        return Path.Combine(tempBase, fileName);
    }

    private HttpContext GetHttpContext()
    {
        return _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HttpContext.");
    }
}
