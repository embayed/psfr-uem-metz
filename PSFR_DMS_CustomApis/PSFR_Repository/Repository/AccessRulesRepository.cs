using Microsoft.EntityFrameworkCore;
using PSFR_Repository.Context;
using PSFR_Repository.Entities.AccessRules;
using PSFR_Repository.Repository.Interfaces;

namespace PSFR_Repository.Repository
{
    public class AccessRulesRepository : IAccessRulesRepository
    {
        private readonly DMSCustomContext _db;

        public AccessRulesRepository(DMSCustomContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(long createdByUserId, int contentTypeId, string contentTypeText, IEnumerable<(string fieldKey, string op, string value)> conditions, IEnumerable<(string type, int externalId, string name)> targets, CancellationToken cancellationToken)
        {
            if (contentTypeId <= 0)
                throw new ArgumentException("ContentTypeId must be > 0.", nameof(contentTypeId));

            if (string.IsNullOrWhiteSpace(contentTypeText))
                throw new ArgumentException("ContentTypeText is required.", nameof(contentTypeText));

            AccessRule rule = new AccessRule
            {
                ContentTypeId = contentTypeId,
                ContentTypeText = contentTypeText.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedAtUtc = DateTime.UtcNow
            };

            List<AccessRuleCondition> conditionEntities = conditions.Select(c => new AccessRuleCondition
            {
                FieldKey = (c.fieldKey ?? string.Empty).Trim(),
                Op = (c.op ?? string.Empty).Trim(),
                Value = (c.value ?? string.Empty).Trim()
            }).ToList();

            List<AccessRuleTarget> targetEntities = targets.Select(t => new AccessRuleTarget
            {
                Type = (t.type ?? string.Empty).Trim(),
                ExternalId = t.externalId,
                Name = (t.name ?? string.Empty).Trim()
            }).ToList();

            rule.Conditions = conditionEntities;
            rule.Targets = targetEntities;

            _db.AccessRules.Add(rule);
            await _db.SaveChangesAsync(cancellationToken);

            return rule.Id;
        }

        public async Task<bool> UpdateAsync(int id, int contentTypeId, string contentTypeText, IEnumerable<(string fieldKey, string op, string value)> conditions, IEnumerable<(string type, int externalId, string name)> targets, CancellationToken cancellationToken)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be > 0.", nameof(id));

            if (contentTypeId <= 0)
                throw new ArgumentException("ContentTypeId must be > 0.", nameof(contentTypeId));

            if (string.IsNullOrWhiteSpace(contentTypeText))
                throw new ArgumentException("ContentTypeText is required.", nameof(contentTypeText));

            AccessRule? rule = await _db.AccessRules.Include(x => x.Conditions).Include(x => x.Targets).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (rule == null)
                return false;

            rule.ContentTypeId = contentTypeId;
            rule.ContentTypeText = contentTypeText.Trim();

            if (rule.Conditions.Count > 0)
                _db.AccessRuleConditions.RemoveRange(rule.Conditions);

            if (rule.Targets.Count > 0)
                _db.AccessRuleTargets.RemoveRange(rule.Targets);

            List<AccessRuleCondition> newConditions = conditions.Select(c => new AccessRuleCondition
            {
                AccessRuleId = rule.Id,
                FieldKey = (c.fieldKey ?? string.Empty).Trim(),
                Op = (c.op ?? string.Empty).Trim(),
                Value = (c.value ?? string.Empty).Trim()
            }).ToList();

            List<AccessRuleTarget> newTargets = targets.Select(t => new AccessRuleTarget
            {
                AccessRuleId = rule.Id,
                Type = (t.type ?? string.Empty).Trim(),
                ExternalId = t.externalId,
                Name = (t.name ?? string.Empty).Trim()
            }).ToList();

            await _db.AccessRuleConditions.AddRangeAsync(newConditions, cancellationToken);
            await _db.AccessRuleTargets.AddRangeAsync(newTargets, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<AccessRuleDto>> ListAsync(int? contentTypeId, CancellationToken cancellationToken)
        {
            IQueryable<AccessRule> query = _db.AccessRules.AsNoTracking().Include(x => x.Conditions).Include(x => x.Targets);

            if (contentTypeId.HasValue)
            {
                query = query.Where(x => x.ContentTypeId == contentTypeId.Value);
            }

            List<AccessRule> rules = await query.OrderByDescending(x => x.Id).ToListAsync(cancellationToken);

            List<AccessRuleDto> result = rules.Select(x => new AccessRuleDto
            {
                Id = x.Id,
                ContentTypeId = x.ContentTypeId,
                ContentTypeText = x.ContentTypeText,
                Conditions = x.Conditions.Select(c => new AccessRuleConditionDto { FieldKey = c.FieldKey, Op = c.Op, Value = c.Value }).ToList(),
                Targets = x.Targets.Select(t => new AccessRuleTargetDto { Type = t.Type, ExternalId = t.ExternalId, Name = t.Name }).ToList()
            }).ToList();

            return result;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            AccessRule? rule = await _db.AccessRules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (rule == null)
                return false;

            _db.AccessRules.Remove(rule);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
