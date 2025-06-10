using MongoDB.Bson.Serialization.Attributes;

namespace CommentsApi.Models;

public class Counter
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("seq")]
    public int SequenceValue { get; set; }
}
