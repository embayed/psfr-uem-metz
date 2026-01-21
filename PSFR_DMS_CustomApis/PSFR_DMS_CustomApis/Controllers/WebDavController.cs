using Microsoft.AspNetCore.Mvc;
using PSFR_DMS_CustomApis.Attributes;
using PSFR_EditInDesktop.Attributes;
using PSFR_EditInDesktop.Constants;
using PSFR_EditInDesktop.Services;

namespace PSFR_DMS_CustomApis.Controllers;

/// <summary>
/// WebDAV controller that integrates with Viewer API
/// Implements checkout/checkin workflow for Office applications
/// </summary>
[ApiController]
[Route(WebDavConstants.WebDavRouteBase)]
public class WebDavController(IWebDavService webDavService) : ControllerBase
{
    private readonly IWebDavService _webDavService = webDavService;

    /// <summary>
    /// Get WebDAV URL for Office - this builds the office:// URI
    /// GET /webdav/{documentId}/edit-url?mode=edit or mode=view
    /// </summary>
    [HttpGet("{documentId}/edit-url")]
    public Task<IActionResult> GetEditUrl(string documentId,
                                          [FromQuery] string version = WebDavConstants.VersionCurrent,
                                          [FromQuery] bool fromAttachment = false,
                                          [FromQuery] string token = "",
                                          [FromQuery] string mode = WebDavConstants.ModeEdit)
    {
        return _webDavService.GetEditUrlAsync(documentId, version, fromAttachment, token, mode);
    }

    /// <summary>
    /// WebDAV GET - Fetches file content from Viewer API
    /// GET /webdav/{documentId}/{fileName}
    /// </summary>
    [HttpGet("{documentId}/{fileName}")]
    public Task<IActionResult> GetFile(string documentId, string fileName)
    {
        return _webDavService.GetFileAsync(documentId, fileName);
    }

    /// <summary>
    /// WebDAV PUT - Saves file back to Viewer API (checkin)
    /// PUT /webdav/{documentId}/{fileName}
    /// </summary>
    [HttpPut("{documentId}/{fileName}")]
    public Task<IActionResult> PutFile(string documentId, string fileName)
    {
        return _webDavService.PutFileAsync(documentId, fileName);
    }

    /// <summary>
    /// WebDAV LOCK - Checkout file from Viewer API
    /// </summary>
    [HideMethodFromSwaggerIfConfigured]
    [HttpLock("{documentId}/{fileName}")]
    public Task<IActionResult> Lock(string documentId, string fileName)
    {
        return _webDavService.LockAsync(documentId, fileName);
    }

    /// <summary>
    /// WebDAV UNLOCK - Checkin file without changes
    /// </summary>
    [HideMethodFromSwaggerIfConfigured]
    [HttpUnlock("{documentId}/{fileName}")]
    public Task<IActionResult> Unlock(string documentId, string fileName)
    {
        return _webDavService.UnlockAsync(documentId, fileName);
    }

    /// <summary>
    /// WebDAV OPTIONS - Required for Office to understand the endpoint
    /// </summary>
    [HttpOptions()]
    public IActionResult Options()
    {
        return _webDavService.Options();
    }

    /// <summary>
    /// WebDAV HEAD - Returns file info without content
    /// </summary>
    [HttpHead("{documentId}/{fileName}")]
    public Task<IActionResult> Head(string documentId, string fileName)
    {
        return _webDavService.HeadAsync(documentId, fileName);
    }

    /// <summary>
    /// WebDAV PROPFIND - Returns file properties (required by Office)
    /// </summary>
    [HideMethodFromSwaggerIfConfigured]
    [AcceptVerbs("PROPFIND")]
    [Route("{documentId}/{fileName}")]
    public Task<IActionResult> PropFind(string documentId, string fileName)
    {
        return _webDavService.PropFindAsync(documentId, fileName);
    }
}
