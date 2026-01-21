namespace PSFR_Repository.Models
{
    public sealed class MetadataTreeRequestDto
    {
        public int FileContentTypeId { get; set; }
        public List<string> OrderedFields { get; set; } = new List<string>();
        public List<string> Path { get; set; } = new List<string>();
    }
}
