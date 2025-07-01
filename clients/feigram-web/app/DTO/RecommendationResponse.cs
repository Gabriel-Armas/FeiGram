using System.Text.Json.Serialization;
using app.DTO;

namespace app.DTO
{
    public class RecommendationResponse
    {
        [JsonPropertyName("posts")]
        public List<PostDTO> Posts { get; set; }
    }
}
