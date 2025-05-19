using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LikesApi.Models
{
    public class Counter
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;

        public int SequenceValue { get; set; }
    }
}
