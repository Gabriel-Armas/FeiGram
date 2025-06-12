using MongoDB.Bson.Serialization.Attributes;

namespace CommentsApi.Models;

public class Comment
{
    public int CommentId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string PostId { get; set; } = string.Empty;

    public string TextComment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
