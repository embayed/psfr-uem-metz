namespace PSFR_Repository.Models.AccessRules
{
    public class AccessRuleUpdateRequest
    {
        public int Id { get; set; }
        public int ContentTypeId { get; set; }
        public string ContentTypeText { get; set; } = string.Empty;

        public List<ConditionDto> Conditions { get; set; } = new List<ConditionDto>();
        public List<TargetDto> Targets { get; set; } = new List<TargetDto>();
    }
}
