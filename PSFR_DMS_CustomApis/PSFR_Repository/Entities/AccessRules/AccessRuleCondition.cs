namespace PSFR_Repository.Entities.AccessRules
{
    public class AccessRuleCondition
    {
        public int Id { get; set; }

        public int AccessRuleId { get; set; }

        public string FieldKey { get; set; } = string.Empty;
        public string Op { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public virtual AccessRule AccessRule { get; set; } = null!;
    }
}
