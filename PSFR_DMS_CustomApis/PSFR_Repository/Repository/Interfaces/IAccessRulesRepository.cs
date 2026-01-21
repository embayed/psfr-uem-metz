namespace PSFR_Repository.Repository.Interfaces
{
    public interface IAccessRulesRepository
    {
        Task<int> CreateAsync(long createdByUserId, int contentTypeId, string contentTypeText, IEnumerable<(string fieldKey, string op, string value)> conditions, IEnumerable<(string type, int externalId, string name)> targets, CancellationToken cancellationToken);

        Task<List<AccessRuleDto>> ListAsync(int? contentTypeId, CancellationToken cancellationToken);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(int id, int contentTypeId, string contentTypeText, IEnumerable<(string fieldKey, string op, string value)> conditions, IEnumerable<(string type, int externalId, string name)> targets, CancellationToken cancellationToken);

    }

    public class AccessRuleDto
    {
        public int Id { get; set; }
        public int ContentTypeId { get; set; }
        public string ContentTypeText { get; set; } = string.Empty;

        public List<AccessRuleConditionDto> Conditions { get; set; } = new List<AccessRuleConditionDto>();
        public List<AccessRuleTargetDto> Targets { get; set; } = new List<AccessRuleTargetDto>();
    }

    public class AccessRuleConditionDto
    {
        public string FieldKey { get; set; } = string.Empty;
        public string Op { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class AccessRuleTargetDto
    {
        public string Type { get; set; } = string.Empty;
        public int ExternalId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
