namespace PSFR_EditInDesktop.Constants;

public static class WebDavConstants
{
    // General
    public const string WebDavRouteBase = "webdav";

    public const string ExternalApiUrlConfigKey = "ExternalApis:DmsBaseUrl";

    public const string VersionCurrent = "current";

    public const string ModeEdit = "edit";
    public const string ModeView = "view";

    public const string OfficeProtocolView = "ofv";
    public const string OfficeProtocolEdit = "ofe";

    public const string ContentTypeApplicationXmlUtf8 = "application/xml; charset=utf-8";
    public const string ContentTypeTextHtml = "text/html";
    public const string ContentTypeApplicationOctetStream = "application/octet-stream";

    // Headers
    public const string HeaderAllow = "Allow";
    public const string HeaderDav = "DAV";
    public const string HeaderMsAuthorVia = "MS-Author-Via";
    public const string HeaderPublic = "Public";
    public const string HeaderAccessControlAllowOrigin = "Access-Control-Allow-Origin";
    public const string HeaderAccessControlAllowMethods = "Access-Control-Allow-Methods";
    public const string HeaderAccessControlAllowHeaders = "Access-Control-Allow-Headers";
    public const string HeaderAcceptRanges = "Accept-Ranges";
    public const string HeaderContentDisposition = "Content-Disposition";
    public const string HeaderContentLength = "Content-Length";
    public const string HeaderContentType = "Content-Type";
    public const string HeaderLockToken = "Lock-Token";

    public const string HeaderAllowMethods = "OPTIONS, GET, PUT, LOCK, UNLOCK";
    public const string DavHeaderValue = "1, 2";
    public const string MsAuthorViaValue = "DAV";
    public const string AccessControlAllowOriginAll = "*";
    public const string HeaderAccessControlAllowHeadersAll = "*";
    public const string AcceptRangesBytes = "bytes";

    // Lock info
    public const string LockOwner = "user";
    public const string LockTimeout = "Second-3600";
    public const string LockDepth = "0";
    public const string LockTokenPrefix = "opaquelocktoken:";

    // Query fragments
    public const string QueryIsVersionModifiedTrue = "isVersionModified=true";
    public const string QueryIsVersionModifiedFalse = "isVersionModified=false";

    // URL templates
    public static class UrlTemplates
    {
        public const string ViewerVersions =
            "{0}/Viewer/file/{1}/versions";

        public const string ViewerFileVersion =
            "{0}/Viewer/file/{1}/version/{2}?fromAttachment={3}";

        public const string ViewerFileContent =
            "{0}/Viewer/file/{1}/version/{2}/content?fromAttachment={3}";

        public const string ViewerFileCheckout =
            "{0}/Viewer/file/{1}/version/{2}/checkout?fromAttachment={3}";

        public const string ViewerFileCheckin =
            "{0}/Viewer/file/{1}/version/{2}/checkin?{3}&fromAttachment={4}";

        public const string WebDavFileUrl =
            "{0}://{1}/" + WebDavRouteBase + "/{2}/{3}";
    }

    // XML templates
    public static class XmlTemplates
    {
        public const string LockResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
<D:prop xmlns:D=""DAV:"">
    <D:lockdiscovery>
        <D:activelock>
            <D:locktype><D:write/></D:locktype>
            <D:lockscope><D:exclusive/></D:lockscope>
            <D:depth>{0}</D:depth>
            <D:owner>{1}</D:owner>
            <D:timeout>{2}</D:timeout>
            <D:locktoken>
                <D:href>{3}</D:href>
            </D:locktoken>
        </D:activelock>
    </D:lockdiscovery>
</D:prop>";

        public const string PropFindResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
<D:multistatus xmlns:D=""DAV:"">
    <D:response>
        <D:href>/{0}/{1}/{2}</D:href>
        <D:propstat>
            <D:prop>
                <D:getcontentlength>{3}</D:getcontentlength>
                <D:getcontenttype>{4}</D:getcontenttype>
                <D:getlastmodified>{5}</D:getlastmodified>
                <D:resourcetype/>
            </D:prop>
            <D:status>HTTP/1.1 200 OK</D:status>
        </D:propstat>
    </D:response>
</D:multistatus>";
    }

    // Error / response messages
    public static class Messages
    {
        public const string FileNotFoundFormat = "File not found: {0}";
        public const string ViewerEmptyFileData = "Viewer API returned empty file data";
        public const string AuthFailedLoginPage = "Authentication failed - API returned login page";
        public const string ServerReturnedTextInsteadOfBinary =
            "Server returned text/JSON instead of binary file content";
        public const string FailedToSaveViewer = "Failed to save to Viewer system";
        public const string ViewModeLocked = "File opened in view-only mode";
    }

    // Log message templates
    public static class LogMessages
    {
        public const string GetEditUrlCalled =
            "GetEditUrl called: {DocumentId}, Version: {Version}, FromAttachment: {FromAttachment}, HasToken: {HasToken}, Mode: {Mode}";

        public const string NoTokenProvidedGetEditUrl =
            "No token provided in GetEditUrl for document {DocumentId}";

        public const string FailedToFetchVersionsFallback =
            "Failed to fetch versions - Status: {StatusCode}, falling back to requested version: {Version}";

        public const string ErrorFetchingVersionsFallback =
            "Error fetching versions, falling back to requested version: {Version}";

        public const string UsingLatestVersion =
            "Using latest version {LatestVersion} instead of requested {RequestedVersion}";

        public const string ViewerApiReturnedStatusForDocument =
            "Viewer API returned {StatusCode} for document {DocumentId}";

        public const string ViewerApiResponse = "Viewer API response: {Response}";

        public const string FileJsonString = "File JSON string: {FileJson}";

        public const string FileJsonStringEmpty =
            "File JSON string is empty for document {DocumentId}";

        public const string ExtractedFileName =
            "Extracted fileName: {FileName}";

        public const string NoTokenToStoreForDocument =
            "No token to store for documentId: {DocumentId}";

        public const string GeneratedOfficeUri =
            "Generated Office URI: {OfficeUri} (Mode: {Mode})";

        public const string WebDavGetRequest =
            "WebDAV GET request: {DocumentId}/{FileName}, Version: {Version}, HasToken: {HasToken}";

        public const string NoTokenInCache =
            "No token found in cache for documentId: {DocumentId}";

        public const string CallingViewerApiUrl =
            "Calling Viewer API: {Url}";

        public const string NoAuthTokenForViewerRequest =
            "No authentication token available for Viewer API request";

        public const string ViewerApiStatus =
            "Viewer API response status: {StatusCode}";

        public const string ServingFileFromViewer =
            "Serving file from Viewer API: {DocumentId}, Size: {Size} bytes, ContentType: {ContentType}";

        public const string FileContentPreview =
            "File content preview (first 500 bytes): {Preview}";

        public const string ReceivedTextInsteadOfBinary =
            "Received text/JSON instead of binary file! Content: {Content}";

        public const string ErrorFetchingFile =
            "Error fetching file from Viewer API: {DocumentId}";

        public const string WebDavPutRequest =
            "WebDAV PUT request: {DocumentId}/{FileName}, Version: {Version}, HasToken: {HasToken}";

        public const string NoTokenForDocument =
            "No token found for documentId: {DocumentId}";

        public const string ReceivedFileForSave =
            "Received file for save: {DocumentId}, Size: {Size} bytes";

        public const string ViewerReturnedWhenSaving =
            "Viewer API returned {StatusCode} when saving document {DocumentId}";

        public const string FileSavedToViewer =
            "File saved to Viewer API successfully: {DocumentId}";

        public const string NewVersionAfterCheckin =
            "New version after checkin: {NewVersion}";

        public const string CouldNotParseCheckinResponse =
            "Could not parse checkin response";

        public const string ErrorSavingFileToViewer =
            "Error saving file to Viewer API: {DocumentId}";

        public const string WebDavLockRequest =
            "WebDAV LOCK request: {DocumentId}/{FileName}, Version: {Version}, Mode: {Mode}";

        public const string NoTokenForLockRequest =
            "No token for LOCK request";

        public const string FailedToCheckoutDocument =
            "Failed to checkout document {DocumentId}. Status: {StatusCode}, Response: {ErrorContent}. Continuing anyway to allow editing.";

        public const string CheckoutSucceeded =
            "Checkout succeeded for document {DocumentId}";

        public const string SkippingCheckoutViewMode =
            "Skipping checkout for view mode: {DocumentId}";

        public const string ViewModeReturningLocked =
            "View mode - returning 423 Locked to prevent editing: {DocumentId}";

        public const string FileLockedSuccessfully =
            "File locked successfully: {DocumentId}, LockToken: {LockToken}";

        public const string ErrorLockingFile =
            "Error locking file: {DocumentId}";

        public const string WebDavUnlockRequest =
            "WebDAV UNLOCK request: {DocumentId}/{FileName}, Version: {Version}, Mode: {Mode}";

        public const string FileStillCheckedOutCallingCheckin =
            "File is still checked out - calling checkin without changes for {DocumentId}";

        public const string FailedToUnlockDocument =
            "Failed to unlock document {DocumentId}. Status: {StatusCode}";

        public const string ViewModeSkippingCheckinUnlock =
            "View mode - skipping checkin on UNLOCK for {DocumentId}";

        public const string FileAlreadyCheckedInUnlock =
            "File already checked in via PUT - skipping checkin on UNLOCK for {DocumentId}";

        public const string FileUnlockedSuccessfully =
            "File unlocked successfully: {DocumentId}";

        public const string ErrorUnlockingFile =
            "Error unlocking file: {DocumentId}";
    }
}
