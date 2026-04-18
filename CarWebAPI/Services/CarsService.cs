using CarWebAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Drawing;
using CarWebAPI.Enums;
using CarWebAPI.DTO;
using CarWebAPI.Telemetry;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Text.Json;
using System.Diagnostics;

namespace CarWebAPI.Services
{
    public class CarsService
    {
        private readonly IMongoCollection<Car> _carsCollection;
        private readonly IDistributedCache _cache;
        private readonly string cachePrefix;

        public CarsService(IOptions<CarSelectionDatabaseSettings> carSelectionDatabaseSettings, 
            IDistributedCache cache,
            IOptions<RedisCacheOptions> redisOptions)
        {
            var mongoClient = new MongoClient
                (carSelectionDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase
                (carSelectionDatabaseSettings.Value.DatabaseName);

            _carsCollection = mongoDatabase.GetCollection<Car>
                (carSelectionDatabaseSettings.Value.CarsCollectionName);
            _cache = cache;
            cachePrefix = redisOptions.Value.InstanceName!;

        }

        public async Task<List<Car>> GetAsync(int sampleSize = 1000)
        {
            //var sw = Stopwatch.StartNew();
            List<Car> results;
            string cacheKey = $"{cachePrefix}_All_{sampleSize}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                results =  System.Text.Json.JsonSerializer.Deserialize<List<Car>>(cachedData) ?? new List<Car>();
            }
            else
            {
                var pipline = new EmptyPipelineDefinition<Car>().Sample(sampleSize);
                results = await _carsCollection.Aggregate(pipline).ToListAsync();
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(results),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
            }
            //sw.Stop();
            //CarMetrics.CarsRequestDuration.Record(sw.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>[] { new ("method",  "GetAll")}); 
            return results;
        }

        public async Task<Car?> GetAsync(string id)
        {
            //var sw = Stopwatch.StartNew();
            Car car;
            string cacheKey = $"{cachePrefix}_ById_{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                car = JsonSerializer.Deserialize<Car>(cachedData);
            }
            else
            {
                car = await _carsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (car != null)
                {
                    await _cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(car),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                        });
                }
            }
            //sw.Stop();
            //CarMetrics.CarsRequestDuration.Record(sw.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>[] { new ("method",  "GetById")});
            return car;
        }
        //public async Task<List<Car>> FilterAsync(decimal lowerPrice, decimal higherPrice)
        //{
        //    var filter = Builders<Car>.Filter.Gte(x => x.Price, lowerPrice) &
        //                 Builders<Car>.Filter.Lte(x => x.Price, higherPrice);
        //    List<Car> results = await _carsCollection.Find(filter).ToListAsync();
        //    return results;

        //}
        //public async Task<List<Car>> FilterAsync(CarColor color)
        //{
        //    var filter = Builders<Car>.Filter.Eq(x => x.Color, color);
        //    var results = await _carsCollection.Find(filter).ToListAsync();
        //    return results;
        //}
        //public async Task<List<Car>> FilterAsync(BodyType BType)
        //{
        //    var filter = Builders<Car>.Filter.Eq(x => x.BodyType, BType);
        //    var results = await _carsCollection.Find(filter).ToListAsync();
        //    return results;
        //}

        public async Task<List<Car>> FilterAsync(CarFilterDTO filterDTO)
        {
            //var sw = Stopwatch.StartNew();
            List<Car> results;
            string filterKey = JsonSerializer.Serialize(filterDTO);
            string cacheKey = $"{cachePrefix}_Filter_{filterKey}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                results = JsonSerializer.Deserialize<List<Car>>(cachedData) ?? new List<Car>();
            }
            else
            {
                var builder = Builders<Car>.Filter;
                var filters = new List<FilterDefinition<Car>>();
                if (filterDTO.MinPrice.HasValue)
                    filters.Add(builder.Gte(x => x.Price, filterDTO.MinPrice.Value));
                if (filterDTO.MaxPrice.HasValue)
                    filters.Add(builder.Lte(x => x.Price, filterDTO.MaxPrice.Value));
                if (filterDTO.Color.HasValue)
                    filters.Add(builder.Eq(x => x.Color, filterDTO.Color.Value));
                if (filterDTO.BodyType.HasValue)
                    filters.Add(builder.Eq(x => x.BodyType, filterDTO.BodyType.Value));
                if (!string.IsNullOrEmpty(filterDTO.Brand))
                    filters.Add(builder.Eq(x => x.Brand, filterDTO.Brand));
                if (!string.IsNullOrEmpty(filterDTO.Model))
                    filters.Add(builder.Eq(x => x.Model, filterDTO.Model));
                if (filterDTO.Year.HasValue)
                    filters.Add(builder.Eq(x => x.Year, filterDTO.Year.Value));
                var combinedFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;
                results = await _carsCollection.Find(combinedFilter).ToListAsync();
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(results),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
            }
            //sw.Stop();
            //CarMetrics.CarsRequestDuration.Record(sw.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>[] { new ("method",  "Filter")});
            return results;
        }
        public async Task CreateAsync(Car newCar)
        {
            await _carsCollection.InsertOneAsync(newCar);
            await _cache.RemoveAsync($"{cachePrefix}_All_1000");
        }

        public async Task UpdateAsync(string id, Car updatedCar)
        {
            await _carsCollection.ReplaceOneAsync(x => x.Id == id, updatedCar);
            await _cache.RemoveAsync($"{cachePrefix}_ById_{id}");
        }

        public async Task RemoveAsync(string id)
        {
            CarMetrics.CarsDeletedCounter.Add(1);
            await _carsCollection.DeleteOneAsync(x => x.Id == id);
            await _cache.RemoveAsync($"{cachePrefix}_All_1000");
            await _cache.RemoveAsync($"{cachePrefix}_ById_{id}");
        }

        public async Task RemoveAllAsync()
        {
            int count = (int)await _carsCollection.CountDocumentsAsync(Builders<Car>.Filter.Empty);
            CarMetrics.CarsDeletedCounter.Add(count);
            List<Car> cars = await _carsCollection.Find(Builders<Car>.Filter.Empty).ToListAsync();
            foreach(var car in cars)
            {
                string cacheKey = $"{cachePrefix}_ById_{car.Id}";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                   await _cache.RemoveAsync(cacheKey);
                }
            }
            await _carsCollection.DeleteManyAsync(Builders<Car>.Filter.Empty);
            await _cache.RemoveAsync($"{cachePrefix}_All_1000");
        }

        public async Task<int> CountAsync()
        {
            int count = (int)await _carsCollection.CountDocumentsAsync(Builders<Car>.Filter.Empty);
            return count;
        }

        public async Task InvalidateListCacheAsync()
        {
            await _cache.RemoveAsync($"{cachePrefix}_All_1000");
        }
    }
}
