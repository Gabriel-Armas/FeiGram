using CommentsApi.Models;
using MongoDB.Driver;

namespace CommentsApi.Data;

public class MongoDbContext
{
private readonly IMongoDatabase _database;
    public IMongoCollection<Comment> Comments { get; }
    public IMongoCollection<Counter> Counters { get; }

    public MongoDbContext(string connectionString)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("feigram");
        Comments = _database.GetCollection<Comment>("comments");
        Counters = _database.GetCollection<Counter>("counters");
    }

    public async Task<int> GetNextSequenceValue(string sequenceName)
    {
        var filter = Builders<Counter>.Filter.Eq(c => c.Id, sequenceName);
        var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
        var options = new FindOneAndUpdateOptions<Counter>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var counter = await Counters.FindOneAndUpdateAsync(filter, update, options);
        return counter.SequenceValue;
    }
}
