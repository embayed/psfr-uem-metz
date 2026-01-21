namespace PSFR_Repository.Entities;

public partial class LookupItems
{
    public int Id { get; set; }

    public short? ParentId { get; set; }

    public int? LookupItemParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameFr { get; set; }

    public string? NameAr { get; set; }

    public long CreatedByUserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Code { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual Lookup? Lookup { get; set; }
}
