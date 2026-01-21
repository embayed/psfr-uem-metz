namespace PSFR_Repository.Entities.AccessRules
{
    public class AccessRuleTarget
    {
        public int Id { get; set; }

        public int AccessRuleId { get; set; }

        public string Type { get; set; } = string.Empty; // User / Group / Role
        public int ExternalId { get; set; }
        public string Name { get; set; } = string.Empty;

        public virtual AccessRule AccessRule { get; set; } = null!;
    }
}
