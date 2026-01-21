using System.ComponentModel.DataAnnotations;

namespace PSFR_Repository.Models
{
    public sealed class MetadataTreeUserConfig
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public int? FileContentTypeId { get; set; }

        [Required]
        public string OrderedFieldsJson { get; set; } = "[]";

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
