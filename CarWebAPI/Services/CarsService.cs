using CarWebAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Drawing;
using CarWebAPI.Enums;
using CarWebAPI.DTO;
using CarWebAPI.Telemetry;

namespace CarWebAPI.Services
{
    public class CarsService
    {
        private readonly IMongoCollection<Car> _carsCollection;

        public CarsService(IOptions<CarSelectionDatabaseSettings> carSelectionDatabaseSettings)
        {
            var mongoClient = new MongoClient
                (carSelectionDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase
                (carSelectionDatabaseSettings.Value.DatabaseName);

            _carsCollection = mongoDatabase.GetCollection<Car>
                (carSelectionDatabaseSettings.Value.CarsCollectionName);
        }

        public async Task<List<Car>> GetAsync(int sampleSize = 1000)
        {
            var pipline = new EmptyPipelineDefinition<Car>().Sample(sampleSize);
            List<Car> results = await _carsCollection.Aggregate(pipline).ToListAsync();
            return results;
        }

        public async Task<Car?> GetAsync(string id) =>
            await _carsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
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
            return await _carsCollection.Find(combinedFilter).ToListAsync();
        }
        public async Task CreateAsync(Car newCar) =>
            await _carsCollection.InsertOneAsync(newCar);

        public async Task UpdateAsync(string id, Car updatedCar) =>
            await _carsCollection.ReplaceOneAsync(x => x.Id == id, updatedCar);

        public async Task RemoveAsync(string id)
        {
            CarMetrics.CarsDeletedCounter.Add(1);
            await _carsCollection.DeleteOneAsync(x => x.Id == id);
        }

        public async Task RemoveAllAsync()
        {
            int count = (int)await _carsCollection.CountDocumentsAsync(Builders<Car>.Filter.Empty);
            CarMetrics.CarsDeletedCounter.Add(count);
            await _carsCollection.DeleteManyAsync(Builders<Car>.Filter.Empty);
        }

        public async Task<int> CountAsync()
        {
            int count = (int)await _carsCollection.CountDocumentsAsync(Builders<Car>.Filter.Empty);
            return count;
        }
    }
}
