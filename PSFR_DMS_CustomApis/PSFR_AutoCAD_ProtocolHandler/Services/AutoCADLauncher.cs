using Microsoft.Win32;
using PSFR_AutoCAD_ProtocolHandler.Constants;
using PSFR_AutoCAD_ProtocolHandler.Models;
using PSFR_AutoCAD_ProtocolHandler.Utilitys;
using System.Diagnostics;

namespace PSFR_AutoCAD_ProtocolHandler.Services;

public static class AutoCADLauncher
{
    private static readonly string TempFolder = Path.Combine(Path.GetTempPath(), ApplicationConstants.Paths.TempFolderName);
    private static readonly Dictionary<string, FileMonitor> ActiveMonitors = new();
    private static TaskCompletionSource<bool>? monitoringComplete;

    public static async Task HandleFileOpenAsync(string webDavUrl, bool isEditMode)
    {
        try
        {
            Logger.ClearOldLogs();

            // Ignore test probes from protocol detection
            if (string.IsNullOrWhiteSpace(webDavUrl) || webDavUrl.Equals("test", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info("Protocol detection test received - exiting silently");
                return;
            }

            Uri uri = new(webDavUrl);
            Dictionary<string, string> queryParams = ParseQueryString(uri.Query);

            string documentId = queryParams.GetValueOrDefault("documentId", string.Empty);
            string token = queryParams.GetValueOrDefault("token", string.Empty);
            string version = queryParams.GetValueOrDefault("version", "current");
            bool fromAttachment = bool.Parse(queryParams.GetValueOrDefault("fromAttachment", "false"));
            string viewerApiUrl = queryParams.GetValueOrDefault("viewerApiUrl", string.Empty);

            string downloadUrl = webDavUrl.Split('?')[0];

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }

            string fileName = GetFileNameFromUrl(downloadUrl);
            string localFilePath = Path.Combine(TempFolder, fileName);
            localFilePath = CleanupAndGetTempFilePath(localFilePath, fileName);

            // Checkout file if in edit mode
            if (isEditMode)
            {
                if (!string.IsNullOrEmpty(documentId) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(viewerApiUrl))
                {
                    await CheckoutFileAsync(viewerApiUrl, documentId, version, fromAttachment, token);
                }
                else
                {
                    Logger.Warning($"Cannot checkout: Missing parameters - DocumentId:{!string.IsNullOrEmpty(documentId)}, Token:{!string.IsNullOrEmpty(token)}, ViewerApiUrl:{!string.IsNullOrEmpty(viewerApiUrl)}");
                    MessageBox.Show(
                        "Cannot checkout file: Missing required parameters\n" +
                        $"DocumentId: {(!string.IsNullOrEmpty(documentId) ? "✓" : "✗")}\n" +
                        $"Token: {(!string.IsNullOrEmpty(token) ? "✓" : "✗")}\n" +
                        $"ViewerApiUrl: {(!string.IsNullOrEmpty(viewerApiUrl) ? "✓" : "✗")}",
                        "Checkout Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }

            using (HttpClient client = new())
            {
                client.Timeout = TimeSpan.FromMinutes(ApplicationConstants.Timeouts.DownloadTimeoutMinutes);
                HttpResponseMessage response = await client.GetAsync(downloadUrl);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Logger.Error($"Download failed: HTTP {(int)response.StatusCode} - {errorContent}");
                    throw new Exception($"Failed to download file (HTTP {(int)response.StatusCode} {response.StatusCode}):\n\n" +
                                      $"URL: {webDavUrl}\n\n" +
                                      $"Server response:\n{errorContent}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType?.Contains("text/html") == true || contentType?.Contains("text/xml") == true)
                {
                    string htmlContent = await response.Content.ReadAsStringAsync();
                    Logger.Error($"Server returned HTML/XML instead of file. Content-Type: {contentType}");
                    throw new Exception($"Server returned HTML/XML instead of a file. Check URL and permissions.\n\n" +
                                      $"URL: {webDavUrl}\n" +
                                      $"Content-Type: {contentType}\n\n" +
                                      $"Response preview:\n{htmlContent.Substring(0, Math.Min(500, htmlContent.Length))}...");
                }

                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(localFilePath, fileBytes);
            }

            LaunchAutoCAD(localFilePath, downloadUrl, documentId, token, version, fromAttachment, viewerApiUrl, isEditMode);

            if (isEditMode && monitoringComplete != null)
            {
                await monitoringComplete.Task;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Fatal error in HandleFileOpenAsync", ex);
            MessageBox.Show(
                $"Error opening file in AutoCAD:\n\n{ex.Message}\n\nCheck log file at:\n{Logger.GetLogFilePath()}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static string GetFileNameFromUrl(string url)
    {
        try
        {
            Uri uri = new Uri(url);
            string[] segments = uri.Segments;
            string lastSegment = segments[segments.Length - 1];
            return Uri.UnescapeDataString(lastSegment);
        }
        catch
        {
            return $"drawing_{DateTime.Now:yyyyMMddHHmmss}.dwg";
        }
    }

    private static void LaunchAutoCAD(string filePath, string webDavUrl, string documentId, string token, string version, bool fromAttachment, string viewerApiUrl, bool isEditMode)
    {
        string? autocadPath = FindAutoCADPath();

        if (string.IsNullOrEmpty(autocadPath))
        {
            Logger.Error("AutoCAD installation not found");
            MessageBox.Show(
                "AutoCAD installation not found. Please ensure AutoCAD is installed on this computer.",
                "AutoCAD Not Found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = autocadPath,
                Arguments = $"\"{filePath}\"",
                UseShellExecute = false
            };

            if (!isEditMode)
            {
                File.SetAttributes(filePath, FileAttributes.ReadOnly);
            }

            Process? process = Process.Start(startInfo);

            if (process != null && isEditMode)
            {
                StartMonitoring(filePath, webDavUrl, documentId, token, version, fromAttachment, viewerApiUrl, process);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to launch AutoCAD", ex);
            MessageBox.Show(
                $"Failed to launch AutoCAD:\n\n{ex.Message}\n\nCheck log file at:\n{Logger.GetLogFilePath()}",
                "Launch Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static void StartMonitoring(string filePath, string webDavUrl, string documentId, string token, string version, bool fromAttachment, string viewerApiUrl, Process process)
    {
        try
        {
            monitoringComplete = new TaskCompletionSource<bool>();
            StopMonitoring(filePath);

            FileInfo fileInfo = new FileInfo(filePath);
            string initialHash = CalculateFileHash(filePath);

            var monitor = new FileMonitor
            {
                FilePath = filePath,
                WebDavUrl = webDavUrl,
                DocumentId = documentId,
                Token = token,
                Version = version,
                FromAttachment = fromAttachment,
                ViewerApiUrl = viewerApiUrl,
                LastModified = fileInfo.LastWriteTime,
                IsEditMode = true,
                AutoCADProcess = process,
                WasModified = false,
                InitialFileHash = initialHash,
                InitialLastWriteTime = fileInfo.LastWriteTime,
                InitialFileSize = fileInfo.Length
            };

            var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath)!)
            {
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            watcher.Changed += (sender, e) =>
            {
                monitor.WasModified = true;
                monitor.LastModified = File.GetLastWriteTime(filePath);
            };

            watcher.EnableRaisingEvents = true;
            monitor.Watcher = watcher;
            ActiveMonitors[filePath] = monitor;

            Task.Run(async () =>
            {
                await process.WaitForExitAsync();
                await OnAutoCADClosed(monitor);
            });
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to start monitoring", ex);
            monitoringComplete?.TrySetResult(false);
        }
    }

    private static async Task<bool> WaitForFileToBeAccessible(string filePath, int maxRetries, int delayMs)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    if (fs.Length == 0)
                    {
                        await Task.Delay(delayMs);
                        continue;
                    }
                    return true;
                }
            }
            catch (IOException)
            {
                await Task.Delay(delayMs);
            }
        }
        Logger.Error($"File not accessible after {maxRetries} retries");
        return false;
    }

    private static async Task OnAutoCADClosed(FileMonitor monitor)
    {
        try
        {
            await Task.Delay(ApplicationConstants.Timeouts.FileWriteDelayMilliseconds);

            if (!await WaitForFileToBeAccessible(
                monitor.FilePath,
                ApplicationConstants.Timeouts.FileAccessRetryCount,
                ApplicationConstants.Timeouts.FileAccessRetryDelayMilliseconds))
            {
                Logger.Error("File is still locked or empty after retries");
                MessageBox.Show(
                    "File is still in use or appears corrupted. Cannot upload to server.\n\n" +
                    "Please ensure AutoCAD has fully saved the file and try again.",
                    "File Access Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            bool fileActuallyModified = false;

            if (File.Exists(monitor.FilePath))
            {
                FileInfo currentFileInfo = new FileInfo(monitor.FilePath);
                string currentHash = CalculateFileHash(monitor.FilePath);

                if (!string.IsNullOrEmpty(currentHash) && !string.IsNullOrEmpty(monitor.InitialFileHash))
                {
                    fileActuallyModified = currentHash != monitor.InitialFileHash;
                }
                else
                {
                    fileActuallyModified = currentFileInfo.Length != monitor.InitialFileSize ||
                                          Math.Abs((currentFileInfo.LastWriteTime - monitor.InitialLastWriteTime).TotalSeconds) > 1;
                }
            }
            else
            {
                Logger.Warning("File no longer exists");
            }

            if (fileActuallyModified)
            {
                await CheckinFileAsync(monitor.ViewerApiUrl, monitor.DocumentId, monitor.Version, monitor.FromAttachment, monitor.Token, true, monitor.FilePath);
            }
            else
            {
                await CheckinFileAsync(monitor.ViewerApiUrl, monitor.DocumentId, monitor.Version, monitor.FromAttachment, monitor.Token, false, null);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("OnAutoCADClosed failed", ex);
            MessageBox.Show(
                $"Failed to checkin file:\n\n{ex.Message}\n\nCheck log file at:\n{Logger.GetLogFilePath()}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            StopMonitoring(monitor.FilePath);

            try
            {
                if (File.Exists(monitor.FilePath))
                {
                    File.Delete(monitor.FilePath);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to delete temp file: {ex.Message}");
            }

            monitoringComplete?.TrySetResult(true);
        }
    }

    private static void StopMonitoring(string filePath)
    {
        if (ActiveMonitors.TryGetValue(filePath, out var monitor))
        {
            monitor.Watcher?.Dispose();
            ActiveMonitors.Remove(filePath);
        }
    }

    private static async Task CheckoutFileAsync(string viewerApiUrl, string documentId, string version, bool fromAttachment, string token)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(ApplicationConstants.Timeouts.CheckoutTimeoutMinutes);

            string checkoutUrl = $"{viewerApiUrl}/Viewer/file/{documentId}/version/{version}/checkout?fromAttachment={fromAttachment}";

            HttpRequestMessage request = new(HttpMethod.Get, checkoutUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(ApplicationConstants.Http.BearerScheme, token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Logger.Error($"Checkout failed: HTTP {(int)response.StatusCode} - {errorContent}");

                string errorMessage = $"Checkout failed!\n\n" +
                                    $"HTTP Status: {(int)response.StatusCode} {response.StatusCode}\n" +
                                    $"URL: {checkoutUrl}\n\n" +
                                    $"Response:\n{errorContent}";

                if ((int)response.StatusCode == 403)
                {
                    errorMessage += "\n\n❌ 403 Forbidden - Possible reasons:\n" +
                                  "• File is already checked out by another user\n" +
                                  "• You don't have permission to checkout\n" +
                                  "• Token has expired or is invalid";
                }

                throw new Exception(errorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Checkout exception", ex);
            throw new Exception($"Failed to checkout file: {ex.Message}", ex);
        }
    }

    private static async Task CheckinFileAsync(string viewerApiUrl, string documentId, string version, bool fromAttachment, string token, bool isVersionModified, string? filePath)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(ApplicationConstants.Timeouts.CheckinTimeoutMinutes);

            string checkinUrl = $"{viewerApiUrl}/Viewer/file/{documentId}/version/{version}/checkin?isVersionModified={isVersionModified}&fromAttachment={fromAttachment}";

            HttpRequestMessage request = new(HttpMethod.Post, checkinUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(ApplicationConstants.Http.BearerScheme, token);

            if (isVersionModified && !string.IsNullOrEmpty(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length == 0)
                {
                    Logger.Error("Cannot checkin: File is 0 bytes");
                    throw new Exception("File is empty (0 bytes). Cannot checkin to prevent corruption.");
                }

                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

                if (fileBytes.Length == 0)
                {
                    Logger.Error("Cannot checkin: File read as 0 bytes");
                    throw new Exception("File read as empty (0 bytes). Checkin aborted to prevent corruption.");
                }

                ByteArrayContent content = new ByteArrayContent(fileBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ApplicationConstants.Http.ContentTypeOctetStream);
                content.Headers.ContentLength = fileBytes.Length;

                request.Content = content;
            }

            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Logger.Error($"Checkin failed: HTTP {(int)response.StatusCode} - {errorContent}");

                string errorMessage = $"Checkin failed!\n\n" +
                                    $"HTTP Status: {(int)response.StatusCode} {response.StatusCode}\n" +
                                    $"URL: {checkinUrl}\n\n" +
                                    $"Response:\n{errorContent}";

                throw new Exception(errorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Checkin exception", ex);
            throw new Exception($"Failed to checkin file: {ex.Message}", ex);
        }
    }

    private static string? FindAutoCADPath()
    {
        string[] autocadExeNames = ApplicationConstants.AutoCAD.ExecutableNames;
        string[] registryPaths = ApplicationConstants.AutoCAD.RegistryPaths;

        foreach (string regPath in registryPaths)
        {
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(regPath))
                {
                    if (key != null)
                    {
                        foreach (string versionKey in key.GetSubKeyNames())
                        {
                            using (RegistryKey? versionSubKey = key.OpenSubKey(versionKey))
                            {
                                if (versionSubKey != null)
                                {
                                    object? acadLocationValue = versionSubKey.GetValue(ApplicationConstants.AutoCAD.RegistryLocationKey);
                                    if (acadLocationValue != null)
                                    {
                                        string acadLocation = acadLocationValue.ToString() ?? string.Empty;
                                        foreach (string exeName in autocadExeNames)
                                        {
                                            string fullPath = Path.Combine(acadLocation, exeName);
                                            if (File.Exists(fullPath))
                                            {
                                                return fullPath;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        string[] commonPaths = ApplicationConstants.AutoCAD.CommonInstallationPaths;

        foreach (string basePath in commonPaths)
        {
            if (Directory.Exists(basePath))
            {
                foreach (string exeName in autocadExeNames)
                {
                    string[] foundFiles = Directory.GetFiles(basePath, exeName, SearchOption.AllDirectories);
                    if (foundFiles.Length > 0)
                    {
                        return foundFiles[0];
                    }
                }
            }
        }

        return null;
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(query))
            return result;

        if (query.StartsWith("?"))
            query = query.Substring(1);

        foreach (var pair in query.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts.Length == 2)
            {
                result[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1]);
            }
        }

        return result;
    }

    private static string CleanupAndGetTempFilePath(string originalPath, string fileName)
    {
        try
        {
            CleanupOldTempFiles();

            if (File.Exists(originalPath))
            {
                try
                {
                    FileAttributes attributes = File.GetAttributes(originalPath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(originalPath, attributes & ~FileAttributes.ReadOnly);
                    }

                    File.Delete(originalPath);
                    return originalPath;
                }
                catch
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    string uniqueFileName = $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    return Path.Combine(TempFolder, uniqueFileName);
                }
            }

            return originalPath;
        }
        catch
        {
            return originalPath;
        }
    }

    private static void CleanupOldTempFiles()
    {
        try
        {
            if (!Directory.Exists(TempFolder))
                return;

            DateTime cutoffTime = DateTime.Now.AddHours(-ApplicationConstants.Cleanup.OldFilesThresholdHours);

            foreach (string file in Directory.GetFiles(TempFolder))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(file);

                    if (fileInfo.LastWriteTime < cutoffTime)
                    {
                        if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            File.SetAttributes(file, fileInfo.Attributes & ~FileAttributes.ReadOnly);
                        }

                        File.Delete(file);
                    }
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }

    private static string CalculateFileHash(string filePath)
    {
        try
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to calculate file hash: {ex.Message}");
            return string.Empty;
        }
    }
}
