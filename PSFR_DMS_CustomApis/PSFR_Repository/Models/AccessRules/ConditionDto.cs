namespace PSFR_Repository.Models.AccessRules
{
    public class ConditionDto
    {
        public string FieldKey { get; set; } = string.Empty;
        public string Op { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
