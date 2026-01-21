using System.ComponentModel.DataAnnotations;

namespace PSFR_Repository.Models
{
    public class LookupItemsViewModel
    {
        public int? Id { get; set; }

        public short? ParentId { get; set; }

        public int? LookupItemParentId { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 1)]
        public required string Name { get; set; }

        [StringLength(150, MinimumLength = 1)]
        public string? NameFr { get; set; }

        [StringLength(150, MinimumLength = 1)]
        public string? NameAr { get; set; }

        [StringLength(10)]
        public string? Code { get; set; }

        public string? CreatedDate { get; set; }

        public string? ModifiedDate { get; set; }
    }
}
