using System.Text.Json.Serialization;
namespace app.DTO
{
    public class Like
    {
        public int Id { get; set; }
        public string PostId { get; set; }

        public string UserId { get; set; }
    }
}

