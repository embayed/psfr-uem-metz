using Newtonsoft.Json;

namespace PSFR_Services.Models
{
    public class ActiveBasicContentType
    {
        [JsonProperty("id")]
        public string? Id { get; set; } = "0";

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }
}
