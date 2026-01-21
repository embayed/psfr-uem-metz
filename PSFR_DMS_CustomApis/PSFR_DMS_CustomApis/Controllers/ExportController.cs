using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PSFR_Repository.Models;
using PSFR_Repository.Services.Interfaces;
using PSFR_Services.Enums;
using PSFR_Services.Services;

namespace PSFR_DMS_CustomApis.Controllers
{
    public class ExportController(IOptions<ExternalApisOptions> options, IExceptionLoggerService exceptionLoggerService) : BaseController(exceptionLoggerService)
    {
        private readonly ExternalApisOptions _externalApisOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));

        [HttpPost("ExportSearchResult")]
        public async Task<IActionResult> ExportSearchResultAsync([FromForm] string searchBody,
                                                                 [FromForm] SearchType searchType,
                                                                 [FromForm] string? selectedFiles,
                                                                 [FromForm] LanguagePS language = LanguagePS.Fr,
                                                                 CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(AccessToken)) return Unauthorized();

            try
            {
                byte[] fileBytes = await ExportAdvancedSearchResults
                    .GenerateExportableSearchResultService(searchBody, searchType, _externalApisOptions.DmsBaseUrl, AccessToken, selectedFiles, language, cancellationToken: cancellationToken);

                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = $"SearchResult_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return HttpAjaxError(ex);
            }
        }
    }
}