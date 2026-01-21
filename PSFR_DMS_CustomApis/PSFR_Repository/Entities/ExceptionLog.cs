namespace PSFR_Repository.Entities;

public partial class ExceptionLog
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public long? PrimaryKeyValue { get; set; }

    public string Exception { get; set; } = null!;

    public string MachineName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? Level { get; set; }
}
