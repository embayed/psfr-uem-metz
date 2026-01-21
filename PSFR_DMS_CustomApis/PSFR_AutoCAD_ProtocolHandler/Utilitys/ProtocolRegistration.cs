using Microsoft.Win32;
using System.Reflection;
using System.Security.Principal;
using PSFR_AutoCAD_ProtocolHandler.Constants;

namespace PSFR_AutoCAD_ProtocolHandler.Utilitys;

public static class ProtocolRegistration
{

    public static void RegisterProtocols()
    {
        // Check if running as administrator
        if (!IsAdministrator())
        {
            throw new UnauthorizedAccessException("This operation requires administrator privileges. Please run as administrator.");
        }

        // Get the actual executable path (works with single-file publish)
        string? exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath))
        {
            // Fallback to assembly location
            exePath = Assembly.GetExecutingAssembly().Location;
            if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                exePath = exePath.Replace(".dll", ".exe");
            }
        }

        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
        {
            throw new Exception($"Could not determine executable path. Path: {exePath}");
        }

        Logger.Info($"Registering protocols with executable: {exePath}");
        RegisterProtocol(ApplicationConstants.Protocols.Edit, "AutoCAD Drawing (Edit)", exePath);
        RegisterProtocol(ApplicationConstants.Protocols.View, "AutoCAD Drawing (View)", exePath);
        Logger.Info("Protocols registered successfully");
    }

    public static void UnregisterProtocols()
    {
        if (!IsAdministrator())
        {
            throw new UnauthorizedAccessException("This operation requires administrator privileges. Please run as administrator.");
        }

        UnregisterProtocol(ApplicationConstants.Protocols.Edit);
        UnregisterProtocol(ApplicationConstants.Protocols.View);
    }

    private static void RegisterProtocol(string protocol, string description, string exePath)
    {
        try
        {
            // Create protocol key
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(protocol))
            {
                key.SetValue("", description);
                key.SetValue(ApplicationConstants.Registry.UrlProtocolValue, "");
                key.SetValue("FriendlyTypeName", description);

                using (RegistryKey iconKey = key.CreateSubKey(ApplicationConstants.Registry.DefaultIconSubKey))
                {
                    iconKey.SetValue("", $"{exePath},0");
                }

                using (RegistryKey commandKey = key.CreateSubKey(ApplicationConstants.Registry.ShellOpenCommandSubKey))
                {
                    commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
                }

                // Set FriendlyAppName to control what browsers show
                using (RegistryKey appKey = key.CreateSubKey(@"Application"))
                {
                    appKey.SetValue("ApplicationName", description);
                    appKey.SetValue("ApplicationDescription", description);
                    appKey.SetValue("ApplicationCompany", "Intalio");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to register protocol '{protocol}': {ex.Message}", ex);
        }
    }

    private static void UnregisterProtocol(string protocol)
    {
        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(protocol, false);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to unregister protocol '{protocol}': {ex.Message}", ex);
        }
    }

    private static bool IsAdministrator()

    {
        try
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
