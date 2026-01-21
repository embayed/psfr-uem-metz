using System.Diagnostics;
using System.Text.RegularExpressions;
using PSFR_AutoCAD_ProtocolHandler.Services;
using PSFR_AutoCAD_ProtocolHandler.Utilitys;

namespace PSFR_AutoCAD_ProtocolHandler;

internal class Program
{
    [STAThread]
    static async Task<int> Main(string[] args)
    {
        try
        {
            Logger.Info($"Application started with {args.Length} arguments");
            if (args.Length > 0)
            {
                Logger.Info($"Arguments: {string.Join(", ", args)}");
            }

            // Check if running with administrative privileges for registration
            if (args.Length > 0)
            {
                string command = args[0].ToLower();

                if (command == "/register" || command == "-register")
                {
                    ProtocolRegistration.RegisterProtocols();
                    MessageBox.Show("AutoCAD protocol handlers registered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                else if (command == "/unregister" || command == "-unregister")
                {
                    ProtocolRegistration.UnregisterProtocols();
                    MessageBox.Show("AutoCAD protocol handlers unregistered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                else if (command.StartsWith("acad-edit:") || command.StartsWith("acad-view:"))
                {
                    // Protocol handler invocation
                    string protocolUrl = args[0];
                    Logger.Info($"Protocol URL received: {protocolUrl}");

                    bool isEditMode = command.StartsWith("acad-edit:");
                    Logger.Info($"Edit mode: {isEditMode}");

                    // Extract the actual file URL (case-insensitive)
                    string fileUrl;
                    int colonIndex = protocolUrl.IndexOf(':', StringComparison.OrdinalIgnoreCase);
                    if (colonIndex > 0 && colonIndex < protocolUrl.Length - 1)
                    {
                        fileUrl = protocolUrl.Substring(colonIndex + 1);
                    }
                    else
                    {
                        Logger.Error($"Invalid protocol URL format: {protocolUrl}");
                        MessageBox.Show($"Invalid protocol URL format: {protocolUrl}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return 1;
                    }

                    Logger.Info($"Extracted file URL: {fileUrl}");

                    await AutoCADLauncher.HandleFileOpenAsync(fileUrl, isEditMode);
                    return 0;
                }
            }

            // No valid arguments
            MessageBox.Show(
                "PSFR AutoCAD Protocol Handler\n\n" +
                "Usage:\n" +
                "  /register   - Register protocol handlers\n" +
                "  /unregister - Unregister protocol handlers\n\n" +
                "This application is designed to be invoked by the browser via custom protocols.",
                "AutoCAD Protocol Handler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error("Unhandled exception in Main", ex);
            MessageBox.Show($"Error: {ex.Message}\n\nCheck log file at:\n{Logger.GetLogFilePath()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }
}
