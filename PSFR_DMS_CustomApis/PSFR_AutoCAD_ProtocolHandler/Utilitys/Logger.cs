using PSFR_AutoCAD_ProtocolHandler.Constants;

namespace PSFR_AutoCAD_ProtocolHandler.Utilitys;

public static class Logger
{
    private static readonly string LogFolder = Path.Combine(Path.GetTempPath(), ApplicationConstants.Paths.TempFolderName);
    private static readonly string LogFile = Path.Combine(LogFolder, ApplicationConstants.Paths.LogFileName);
    private static readonly object lockObject = new();

    static Logger()
    {
        // Ensure log folder exists
        try
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }
        }
        catch
        {
            // Ignore if we can't create the folder
        }
    }

    public static void Info(string message)
    {
        WriteLog("INFO", message);
    }

    public static void Warning(string message)
    {
        WriteLog("WARN", message);
    }

    public static void Error(string message, Exception? ex = null)
    {
        WriteLog("ERROR", message);
        if (ex != null)
        {
            WriteLog("ERROR", $"Exception: {ex.Message}");
            WriteLog("ERROR", $"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                WriteLog("ERROR", $"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }

    public static void Debug(string message)
    {
        WriteLog("DEBUG", message);
    }

    private static void WriteLog(string level, string message)
    {
        try
        {
            lock (lockObject)
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFile, logEntry);
            }
        }
        catch
        {
            // Ignore logging errors - we don't want logging to crash the app
        }
    }

    public static string GetLogFilePath()
    {
        return LogFile;
    }

    public static void ClearOldLogs()
    {
        try
        {
            if (File.Exists(LogFile))
            {
                FileInfo fileInfo = new FileInfo(LogFile);

                // If log file is older than 7 days, delete it
                if (fileInfo.CreationTime < DateTime.Now.AddDays(-7))
                {
                    File.Delete(LogFile);
                }
                // If log file is larger than 10MB, delete it
                else if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    File.Delete(LogFile);
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
