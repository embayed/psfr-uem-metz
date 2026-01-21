namespace PSFR_Repository.Entities;

public partial class User
{
    public long Id { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public int? RoleId { get; set; }

    public bool SecurityBreakedInheritance { get; set; }

    public string? FirstnameAr { get; set; }

    public string? FirstnameFr { get; set; }

    public string? LastnameAr { get; set; }

    public string? LastnameFr { get; set; }



    public virtual ICollection<LookupItems> LookupItems { get; set; } = [];

    public virtual ICollection<Lookup> Lookups { get; set; } = [];
}
