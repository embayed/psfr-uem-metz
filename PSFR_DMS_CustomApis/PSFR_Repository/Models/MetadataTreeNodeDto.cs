using PSFR_Repository.Enums;

namespace PSFR_Repository.Models
{
    public sealed class MetadataTreeNodeDto
    {
        public string Label { get; set; } = string.Empty;
        public MetadataTreeNodeType NodeType { get; set; }
        public long? FileId { get; set; }
        public int ItemsCount { get; set; }
        public bool HasChildren { get; set; }
    }
}
