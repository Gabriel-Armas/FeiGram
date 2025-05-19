using MongoDB.Driver;
using LikesApi.Models;

namespace LikesApi.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        _database = client.GetDatabase("feigram-likes");
    }

    public IMongoCollection<Like> Likes => _database.GetCollection<Like>("likes");
    public IMongoCollection<Counter> Counters => _database.GetCollection<Counter>("counters");

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
