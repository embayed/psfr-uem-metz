using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSFR_Repository.Models.AccessRules;
using PSFR_Repository.Repository.Interfaces;
using PSFR_Repository.Services.Interfaces;

namespace PSFR_DMS_CustomApis.Controllers
{
    [ApiController]
    [Route("api/accessRules")]
    public class AccessRulesController(IExceptionLoggerService exceptionLoggerService, IAccessRulesRepository accessRulesService) : BaseController(exceptionLoggerService)
    {
        private readonly IAccessRulesRepository _accessRulesService = accessRulesService;

        [AllowAnonymous]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] AccessRuleRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Request is required." });

            if (request.ContentTypeId <= 0)
                return BadRequest(new { success = false, message = "ContentTypeId must be > 0." });

            if (string.IsNullOrWhiteSpace(request.ContentTypeText))
                return BadRequest(new { success = false, message = "ContentTypeText is required." });

            long createdByUserId = 1; // TODO: replace with real user id from token/claims when you enable auth

            IEnumerable<(string fieldKey, string op, string value)> conditions = request.Conditions.Select(x => (x.FieldKey, x.Op, x.Value)).ToList();
            IEnumerable<(string type, int externalId, string name)> targets = request.Targets.Select(x => (x.Type, x.Id, x.Name)).ToList();

            int id = await _accessRulesService.CreateAsync(createdByUserId, request.ContentTypeId, request.ContentTypeText, conditions, targets, cancellationToken);

            return Ok(new { success = true, id });
        }

        [AllowAnonymous]
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] AccessRuleUpdateRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Request is required." });

            if (request.Id <= 0)
                return BadRequest(new { success = false, message = "Id must be > 0." });

            if (request.ContentTypeId <= 0)
                return BadRequest(new { success = false, message = "ContentTypeId must be > 0." });

            if (string.IsNullOrWhiteSpace(request.ContentTypeText))
                return BadRequest(new { success = false, message = "ContentTypeText is required." });

            IEnumerable<(string fieldKey, string op, string value)> conditions = request.Conditions.Select(x => (x.FieldKey, x.Op, x.Value)).ToList();
            IEnumerable<(string type, int externalId, string name)> targets = request.Targets.Select(x => (x.Type, x.Id, x.Name)).ToList();

            bool updated = await _accessRulesService.UpdateAsync(request.Id, request.ContentTypeId, request.ContentTypeText, conditions, targets, cancellationToken);

            if (!updated)
                return NotFound(new { success = false, message = "Rule not found.", id = request.Id });

            return Ok(new { success = true, id = request.Id });
        }

        [AllowAnonymous]
        [HttpGet("List")]
        public async Task<IActionResult> List([FromQuery] int? contentTypeId, CancellationToken cancellationToken)
        {
            List<AccessRuleDto> rules = await _accessRulesService.ListAsync(contentTypeId, cancellationToken);

            List<AccessRuleRequest> result = [.. rules.Select(r => new AccessRuleRequest
            {
                Id = r.Id,
                ContentTypeId = r.ContentTypeId,
                ContentTypeText = r.ContentTypeText,
                Conditions = [.. r.Conditions.Select(c => new ConditionDto { FieldKey = c.FieldKey, Op = c.Op, Value = c.Value })],
                Targets = [.. r.Targets.Select(t => new TargetDto { Type = t.Type, Id = t.ExternalId, Name = t.Name })]
            })];

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromQuery] int id, CancellationToken cancellationToken)
        {
            bool deleted = await _accessRulesService.DeleteAsync(id, cancellationToken);

            if (!deleted) return NotFound(new { success = false, message = "Rule not found.", id });

            return Ok(new { success = true, message = "Rule deleted", id });
        }
    }
}
