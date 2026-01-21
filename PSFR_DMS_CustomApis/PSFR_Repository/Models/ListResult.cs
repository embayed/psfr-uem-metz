using Newtonsoft.Json;

namespace PSFR_Repository.Models
{
    public class ListResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public required string Text { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }
    }
}