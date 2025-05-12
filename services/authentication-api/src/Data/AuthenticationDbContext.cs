using AuthenticationApi.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace AuthenticationApi.Data
{
    public class AuthenticationDbContext
    {
        private readonly IMongoDatabase _database;

        public AuthenticationDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        }

        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("Users");
    }
}
