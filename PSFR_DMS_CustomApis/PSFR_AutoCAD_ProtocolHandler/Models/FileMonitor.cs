using System.Diagnostics;

namespace PSFR_AutoCAD_ProtocolHandler.Models;

public class FileMonitor
{
    public string FilePath { get; set; } = string.Empty;
    public string WebDavUrl { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool FromAttachment { get; set; }
    public string ViewerApiUrl { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public bool IsEditMode { get; set; }
    public Process? AutoCADProcess { get; set; }
    public FileSystemWatcher? Watcher { get; set; }
    public bool WasModified { get; set; }
    public string InitialFileHash { get; set; } = string.Empty;
    public DateTime InitialLastWriteTime { get; set; }
    public long InitialFileSize { get; set; }
}
