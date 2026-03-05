using UserWebAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using UserWebAPI.DTO;

namespace UserWebAPI.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _userCollection;
        public UsersService(IOptions<UsersDatabaseSettings> usersDatabaseSettings)
        {
            var mongoClient = new MongoClient
                (usersDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase
                (usersDatabaseSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>
                (usersDatabaseSettings.Value.UsersCollectionName);
        }

        public async Task<List<User>> GetAsync(int sampleSize = 1000)
        {
            var pipeline = new EmptyPipelineDefinition<User>().Sample(sampleSize);
            List<User> results = await _userCollection.Aggregate(pipeline).ToListAsync();
            return results;
        }

        public async Task<User?> GetAsync(string id) =>
            await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        public async Task CreateAsync(User newUser) =>
            await _userCollection.InsertOneAsync(newUser);
        public async Task UpdateAsync(string id, User updateduser) =>
            await _userCollection.ReplaceOneAsync(x => x.Id == id, updateduser);
        public async Task RemoveAsync(string id) =>
            await _userCollection.DeleteOneAsync(x => x.Id == id);
        public async Task RemoveAllAsync() =>
            await _userCollection.DeleteManyAsync(Builders<User>.Filter.Empty);
    }
}
