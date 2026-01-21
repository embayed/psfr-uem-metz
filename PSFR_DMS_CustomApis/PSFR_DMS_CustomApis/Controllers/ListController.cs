using Microsoft.AspNetCore.Mvc;
using PSFR_Repository.Enums;
using PSFR_Repository.Models;
using PSFR_Repository.Services.Interfaces;

namespace PSFR_DMS_CustomApis.Controllers
{
    public class ListController(ILookupItemService lookupItemService, IExceptionLoggerService exceptionLoggerService) : BaseController(exceptionLoggerService)
    {
        private readonly ILookupItemService _lookupItemService = lookupItemService;

        [HttpGet("GetListItemsByListItemParent")]
        public async Task<ActionResult<List<ListResult>>> GetListItemsByListItemParent([FromQuery] string ListName,
                                                                                       [FromQuery] string parentListName = "",
                                                                                       [FromQuery] string parentItemValue = "",
                                                                                       [FromQuery] Language Language = Language.EN)
        {
            List<ListResult> results = await _lookupItemService.GetListItemsByListItemParentAsync(parentListName, ListName, parentItemValue, Language);
            return Ok(results);
        }

        [HttpGet("GetLookupItemsByName")]
        public async Task<ActionResult<List<ListResult>>> GetLookupItemsByName([FromQuery] string ListName,
                                                                               [FromQuery] string search = "",
                                                                               [FromQuery] Language Language = Language.EN)
        {
            List<ListResult> results = await _lookupItemService.GetLookupItemsByNameAsync(ListName, Language, search);
            return Ok(results);
        }
    }
}