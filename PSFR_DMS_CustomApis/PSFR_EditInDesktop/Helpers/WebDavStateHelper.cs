namespace PSFR_EditInDesktop.Helpers;

public class WebDavStateHelper
{
    private static readonly Dictionary<string, WebDavSessionInfo> Sessions = [];
    private static readonly Dictionary<string, string> TokenCache = [];

    public static WebDavSessionInfo? GetSession(string documentId)
    {
        if (Sessions.TryGetValue(documentId, out WebDavSessionInfo? session))
        {
            return session;
        }

        return null;
    }

    public static void SetSession(string documentId, WebDavSessionInfo session)
    {
        Sessions[documentId] = session;
    }

    public static void RemoveSession(string documentId)
    {
        Sessions.Remove(documentId);
    }

    public static string? GetToken(string documentId)
    {
        return TokenCache.TryGetValue(documentId, out string? token)
            ? token
            : null;
    }

    public static void SetToken(string documentId, string token)
    {
        TokenCache[documentId] = token;
    }

    public static void RemoveToken(string documentId)
    {
        TokenCache.Remove(documentId);
    }
}

public record WebDavSessionInfo(string FileName, string Version, bool FromAttachment, string Token, bool IsCheckedOut, string Mode, string? TempFilePath = null);
