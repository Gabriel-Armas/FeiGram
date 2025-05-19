namespace LikesApi.Models;

public class Like
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("postId")]
    public int PostId { get; set; }

    [BsonElement("userId")]
    public int UserId { get; set; }
}
