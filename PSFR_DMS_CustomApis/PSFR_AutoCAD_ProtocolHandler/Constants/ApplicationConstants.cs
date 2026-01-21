namespace PSFR_AutoCAD_ProtocolHandler.Constants;

public static class ApplicationConstants
{
    public static class Protocols
    {
        public const string Edit = "acad-edit";
        public const string View = "acad-view";
    }

    public static class Paths
    {
        public const string TempFolderName = "PSFR_AutoCAD";
        public const string LogFileName = "autocad_handler.log";
    }

    public static class Registry
    {
        public const string UrlProtocolValue = "URL Protocol";
        public const string DefaultIconSubKey = "DefaultIcon";
        public const string ShellOpenCommandSubKey = @"shell\open\command";
    }

    public static class AutoCAD
    {
        public static readonly string[] ExecutableNames = { "acad.exe", "acadlt.exe" };

        public static readonly string[] RegistryPaths =
        {
            @"SOFTWARE\Autodesk\AutoCAD",
            @"SOFTWARE\WOW6432Node\Autodesk\AutoCAD"
        };

        public static readonly string[] CommonInstallationPaths =
        {
            @"C:\Program Files\Autodesk",
            @"C:\Program Files (x86)\Autodesk"
        };

        public const string RegistryLocationKey = "AcadLocation";
    }

    public static class Timeouts
    {
        public const int DownloadTimeoutMinutes = 10;
        public const int UploadTimeoutMinutes = 10;
        public const int CheckoutTimeoutMinutes = 5;
        public const int CheckinTimeoutMinutes = 5;
        public const int FileWriteDelayMilliseconds = 5000; // Increased from 1000 to 5000ms
        public const int FileAccessRetryCount = 10;
        public const int FileAccessRetryDelayMilliseconds = 500;
    }

    public static class Cleanup
    {
        public const int OldFilesThresholdHours = 24;
    }

    public static class Http
    {
        public const string ContentTypeOctetStream = "application/octet-stream";
        public const string BearerScheme = "Bearer";
    }
}
