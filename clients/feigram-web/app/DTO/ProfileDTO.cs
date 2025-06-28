using System.Text.Json.Serialization;

namespace app.DTO
{
    public class ProfileDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("enrollment")]
        public string Enrollment { get; set; } = string.Empty;

        [JsonPropertyName("photo")]
        public string Photo { get; set; } = string.Empty;
    }
}
