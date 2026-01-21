namespace PSFR_Repository.Models.AccessRules
{
    public class TargetDto
    {
        public string Type { get; set; } = string.Empty;
        public int Id { get; set; } // External Id
        public string Name { get; set; } = string.Empty;
    }
}
