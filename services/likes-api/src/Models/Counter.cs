using MongoDB.Bson.Serialization.Attributes;

namespace LikesApi.Models;

public class Counter
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("seq")]
    public int SequenceValue { get; set; }
}
