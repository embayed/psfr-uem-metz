namespace PSFR_Repository.Entities;

public partial class Lookup
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public long CreatedByUserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<LookupItems> LookupItems { get; set; } = new List<LookupItems>();
}
