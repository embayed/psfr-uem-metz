using Microsoft.AspNetCore.Mvc;
using PSFR_Repository.Models;
using PSFR_Repository.Services.Interfaces;

namespace PSFR_DMS_CustomApis.Controllers
{

    [ApiController]
    [Route("api/metadata-tree")]
    public class MetadataTreeController(IMetadataTreeService service, IExceptionLoggerService exceptionLoggerService) : BaseController(exceptionLoggerService)
    {
        private readonly IMetadataTreeService _service = service;

        [HttpPost("level")]
        public async Task<ActionResult<List<MetadataTreeNodeDto>>> GetLevel(MetadataTreeRequestDto request, CancellationToken cancellationToken)
        {
            if (request.FileContentTypeId <= 0 || request.OrderedFields == null || request.OrderedFields.Count == 0)
                return BadRequest("Invalid request.");

            List<MetadataTreeNodeDto> result = await _service.GetLevelAsync(request.FileContentTypeId, request.OrderedFields, request.Path, UserId, RoleId, GroupIds, cancellationToken);

            return Ok(result);
        }
    }
}