using UserWebAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using UserWebAPI.DTO;
using UserWebAPI.Telemetry;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Text.Json;
using System.Diagnostics;

namespace UserWebAPI.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IDistributedCache _cache;
        private readonly string _cachePrefix;

        public UsersService(IOptions<UsersDatabaseSettings> usersDatabaseSettings,
                            IDistributedCache cache,
                            IOptions<RedisCacheOptions> redisOptions)
        {
            var mongoClient = new MongoClient(usersDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(usersDatabaseSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(usersDatabaseSettings.Value.UsersCollectionName);
            _cache = cache;
            _cachePrefix = redisOptions.Value.InstanceName!;
        }

        // Получить случайную выборку (с кэшированием)
        public async Task<List<User>> GetAsync(int sampleSize = 1000)
        {
            var sw = Stopwatch.StartNew();
            List<User> results;
            string cacheKey = $"{_cachePrefix}_All_{sampleSize}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                results = JsonSerializer.Deserialize<List<User>>(cachedData) ?? new List<User>();
            }
            else
            {
                var pipeline = new EmptyPipelineDefinition<User>().Sample(sampleSize);
                results = await _userCollection.Aggregate(pipeline).ToListAsync();
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(results),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }
            sw.Stop();
            UserMetrics.UsersRequestDuration.Record(sw.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("method", "GetAll"));
            return results;
        }

        // Получить пользователя по ID (с кэшированием)
        public async Task<User?> GetAsync(string id)
        {
            var sw = Stopwatch.StartNew();
            User? user = null;
            string cacheKey = $"{_cachePrefix}_ById_{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                user = JsonSerializer.Deserialize<User>(cachedData);
            }
            else
            {
                user = await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (user != null)
                {
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user),
                        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
                }
            }
            sw.Stop();
            UserMetrics.UsersRequestDuration.Record(sw.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("method", "GetById"));
            return user;
        }

        // Создание — сбросить кэш списка
        public async Task CreateAsync(User newUser)
        {
            await _userCollection.InsertOneAsync(newUser);
            UserMetrics.UsersAdded.Add(1);
            await _cache.RemoveAsync($"{_cachePrefix}_All_1000");
        }

        // Обновление — сбросить кэш конкретного пользователя и списка
        public async Task UpdateAsync(string id, User updatedUser)
        {
            await _userCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);
            await _cache.RemoveAsync($"{_cachePrefix}_ById_{id}");
            await _cache.RemoveAsync($"{_cachePrefix}_All_1000");
        }

        // Удаление одного — сбросить кэш
        public async Task RemoveAsync(string id)
        {
            await _userCollection.DeleteOneAsync(x => x.Id == id);
            await _cache.RemoveAsync($"{_cachePrefix}_ById_{id}");
            await _cache.RemoveAsync($"{_cachePrefix}_All_1000");
        }

        // Удаление всех — сбросить кэш списка
        public async Task RemoveAllAsync()
        {
            await _userCollection.DeleteManyAsync(Builders<User>.Filter.Empty);
            await _cache.RemoveAsync($"{_cachePrefix}_All_1000");
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
        }
    }
}