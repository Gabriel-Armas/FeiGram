using System.Text.Json.Serialization;
namespace app.DTO
{
    public class Like
    {
        public int? Id { get; set; }
        [JsonPropertyName("postId")]
        public string PostId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }
}

