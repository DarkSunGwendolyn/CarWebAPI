using CarWebAPI.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace CarWebAPI.Models
{
    public class Car
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Brand { get; set; } = null!;

        public string Model { get; set; } = null!;

        public int Year { get; set; }

        public decimal Price { get; set; }

        [BsonRepresentation(BsonType.String)]
        public BodyType BodyType { get; set; }
        [BsonRepresentation(BsonType.String)]
        public CarColor Color { get; set; }
    }
}
