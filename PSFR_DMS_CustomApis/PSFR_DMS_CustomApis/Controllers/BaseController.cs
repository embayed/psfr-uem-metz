using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSFR_Repository.Enums;
using PSFR_Repository.Services.Interfaces;
using System.Globalization;

namespace PSFR_DMS_CustomApis.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController(IExceptionLoggerService exceptionLoggerService) : ControllerBase
    {
        private readonly IExceptionLoggerService _exceptionLoggerService = exceptionLoggerService;
        protected long UserId => Convert.ToInt64(User.Claims.First(t => t.Type == "Id").Value);
        protected short RoleId => Convert.ToInt16(User.Claims.First(t => t.Type == "RoleId").Value);
        protected byte UserTypeId => Convert.ToByte(User.Claims.First(t => t.Type == "UserTypeId").Value);

        protected List<short> GroupIds
        {
            get
            {
                string? claim = User.Claims.FirstOrDefault(t => t.Type == "GroupIds")?.Value;

                return string.IsNullOrEmpty(claim)
                    ? []
                    : [.. claim.Split('/').Select(short.Parse)];
            }
        }
        protected static Language Language => Enum.Parse<Language>(CultureInfo.CurrentUICulture.Name.Replace("en-GB", "EN").ToUpper());

        protected string? AccessToken
        {
            get
            {
                string? authHeader = Request.Headers.Authorization.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(authHeader)) return null;
                if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;

                return authHeader["Bearer ".Length..].Trim();
            }
        }

        protected IActionResult HttpAjaxError(Exception ex, long? primaryKeyValue = null)
        {
            if (ex is not OperationCanceledException)
            {
                _ = _exceptionLoggerService.LogExceptionAsync(ex, User.Identity?.IsAuthenticated == true ? UserId : null, primaryKeyValue);
            }

            return StatusCode(500, new { ex.Message });
        }

        protected string GetActionUniqueName()
        {
            string controller = ControllerContext.ActionDescriptor.ControllerName;
            string action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller}-{action}";
        }
    }
}
