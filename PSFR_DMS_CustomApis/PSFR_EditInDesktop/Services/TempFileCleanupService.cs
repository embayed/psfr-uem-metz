using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PSFR_EditInDesktop.Services;

/// <summary>
/// Background service that periodically cleans up orphaned temporary files
/// from the WebDAV temp directory.
/// </summary>
public class TempFileCleanupService : BackgroundService
{
    private readonly ILogger<TempFileCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);
    private readonly TimeSpan _fileMaxAge = TimeSpan.FromHours(24);

    public TempFileCleanupService(ILogger<TempFileCleanupService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TempFileCleanupService started. Cleanup interval: {Interval}, Max file age: {MaxAge}",
            _cleanupInterval, _fileMaxAge);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                await CleanupTempFilesAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TempFileCleanupService execution");
                // Continue running despite errors
            }
        }

        _logger.LogInformation("TempFileCleanupService stopped");
    }

    private async Task CleanupTempFilesAsync()
    {
        try
        {
            string tempBase = Path.Combine(Path.GetTempPath(), "WebDav");

            if (!Directory.Exists(tempBase))
            {
                _logger.LogDebug("Temp directory does not exist: {TempBase}", tempBase);
                return;
            }

            _logger.LogInformation("Starting cleanup of temp files in: {TempBase}", tempBase);

            int filesDeleted = 0;
            int directoriesDeleted = 0;
            DateTime cutoffTime = DateTime.Now - _fileMaxAge;

            // Enumerate all document directories
            foreach (string documentDir in Directory.GetDirectories(tempBase))
            {
                try
                {
                    // Delete old files in this directory
                    foreach (string file in Directory.GetFiles(documentDir))
                    {
                        try
                        {
                            FileInfo fileInfo = new(file);

                            if (fileInfo.LastWriteTime < cutoffTime)
                            {
                                File.Delete(file);
                                filesDeleted++;
                                _logger.LogInformation("Deleted orphaned temp file: {FilePath} (Last modified: {LastModified})",
                                    file, fileInfo.LastWriteTime);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not delete temp file: {FilePath}", file);
                        }
                    }

                    // Delete directory if empty
                    if (!Directory.EnumerateFileSystemEntries(documentDir).Any())
                    {
                        try
                        {
                            Directory.Delete(documentDir);
                            directoriesDeleted++;
                            _logger.LogInformation("Deleted empty temp directory: {DirPath}", documentDir);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not delete temp directory: {DirPath}", documentDir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing temp directory: {DirPath}", documentDir);
                }
            }

            if (filesDeleted > 0 || directoriesDeleted > 0)
            {
                _logger.LogInformation("Cleanup completed. Files deleted: {FilesDeleted}, Directories deleted: {DirsDeleted}",
                    filesDeleted, directoriesDeleted);
            }
            else
            {
                _logger.LogDebug("Cleanup completed. No files or directories needed cleanup.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during temp file cleanup");
        }

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TempFileCleanupService is stopping");
        await base.StopAsync(stoppingToken);
    }
}
