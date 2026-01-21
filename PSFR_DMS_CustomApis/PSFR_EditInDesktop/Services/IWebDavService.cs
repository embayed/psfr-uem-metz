using Microsoft.AspNetCore.Mvc;

namespace PSFR_EditInDesktop.Services
{

    public interface IWebDavService
    {
        Task<IActionResult> GetEditUrlAsync(string documentId, string version, bool fromAttachment, string token, string mode);
        Task<IActionResult> GetFileAsync(string documentId, string fileName);
        Task<IActionResult> PutFileAsync(string documentId, string fileName);
        Task<IActionResult> LockAsync(string documentId, string fileName);
        Task<IActionResult> UnlockAsync(string documentId, string fileName);
        Task<IActionResult> HeadAsync(string documentId, string fileName);
        Task<IActionResult> PropFindAsync(string documentId, string fileName);
        IActionResult Options();
    }
}
