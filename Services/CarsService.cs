using CarWebAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Drawing;
using CarWebAPI.Enums;

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
        public async Task<List<Car>> FilterAsync(decimal lowerPrice, decimal higherPrice)
        {
            var filter = Builders<Car>.Filter.Gte(x => x.Price, lowerPrice) &
                         Builders<Car>.Filter.Lte(x => x.Price, higherPrice);
            List<Car> results = await _carsCollection.Find(filter).ToListAsync();
            return results;

        }
        public async Task<List<Car>> FilterAsync(CarColor color)
        {
            var filter = Builders<Car>.Filter.Eq(x => x.Color, color);
            var results = await _carsCollection.Find(filter).ToListAsync();
            return results;
        }
        public async Task<List<Car>> FilterAsync(BodyType BType)
        {
            var filter = Builders<Car>.Filter.Eq(x => x.BodyType, BType);
            var results = await _carsCollection.Find(filter).ToListAsync();
            return results;
        }
        public async Task CreateAsync(Car newCar) =>
            await _carsCollection.InsertOneAsync(newCar);

        public async Task UpdateAsync(string id, Car updatedCar) =>
            await _carsCollection.ReplaceOneAsync(x => x.Id == id, updatedCar);

        public async Task RemoveAsync(string id) =>
            await _carsCollection.DeleteOneAsync(x => x.Id == id);

        public async Task RemoveAllAsync() =>
            await _carsCollection.DeleteManyAsync(Builders<Car>.Filter.Empty);
    }
}
