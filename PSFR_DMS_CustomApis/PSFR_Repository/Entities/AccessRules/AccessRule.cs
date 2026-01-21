namespace PSFR_Repository.Entities.AccessRules
{
    public class AccessRule
    {
        public int Id { get; set; }

        public int ContentTypeId { get; set; }
        public string ContentTypeText { get; set; } = string.Empty;

        public long CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public virtual User CreatedByUser { get; set; } = null!;

        public virtual ICollection<AccessRuleCondition> Conditions { get; set; } = [];
        public virtual ICollection<AccessRuleTarget> Targets { get; set; } = [];
    }
}
