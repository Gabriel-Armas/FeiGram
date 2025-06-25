using MongoDB.Bson.Serialization.Attributes;

namespace CommentsApi.Models;
public class PostCountRequest
{
    public string post_id { get; set; } = string.Empty;
}
