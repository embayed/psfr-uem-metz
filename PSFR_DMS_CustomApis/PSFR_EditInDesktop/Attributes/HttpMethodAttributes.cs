using Microsoft.AspNetCore.Mvc.Routing;

namespace PSFR_EditInDesktop.Attributes;

/// <summary>
/// HTTP LOCK method attribute for WebDAV
/// </summary>
public class HttpLockAttribute(string template) : HttpMethodAttribute(["LOCK"], template)
{
}

/// <summary>
/// HTTP UNLOCK method attribute for WebDAV
/// </summary>
public class HttpUnlockAttribute(string template) : HttpMethodAttribute(["UNLOCK"], template)
{
}
