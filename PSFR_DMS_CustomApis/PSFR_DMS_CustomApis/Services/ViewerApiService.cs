using PSFR_DMS_CustomApis.Controllers;
using PSFR_EditInDesktop.Constants;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PSFR_DMS_CustomApis.Services;

public class ViewerApiService(HttpClient httpClient, ILogger<WebDavController> logger, string viewerApiUrl)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<WebDavController> _logger = logger;
    private readonly string _viewerApiUrl = viewerApiUrl;

    public async Task<string> GetLatestVersionAsync(string documentId, string requestedVersion, string decodedToken)
    {
        string latestVersion = requestedVersion;

        try
        {
            string versionsUrl = string.Format(WebDavConstants.UrlTemplates.ViewerVersions, _viewerApiUrl, documentId);

            var versionsRequest = new HttpRequestMessage(HttpMethod.Get, versionsUrl);
            if (!string.IsNullOrEmpty(decodedToken))
            {
                versionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", decodedToken);
            }

            HttpResponseMessage versionsResponse = await _httpClient.SendAsync(versionsRequest);
            if (versionsResponse.IsSuccessStatusCode)
            {
                string versionsJson = await versionsResponse.Content.ReadAsStringAsync();
                JsonElement versionsList = JsonSerializer.Deserialize<JsonElement>(versionsJson);

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
}
