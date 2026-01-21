namespace PSFR_Services.Models
{
    public class ActiveFileContentType
    {
        public int versionId { get; set; }
        public object? searchForm { get; set; }
        public object? searchFormTranslation { get; set; }
        public string? searchGrid { get; set; }
        public string? mainGridView { get; set; }
        public int id { get; set; }
        public string? text { get; set; }
    }
}
