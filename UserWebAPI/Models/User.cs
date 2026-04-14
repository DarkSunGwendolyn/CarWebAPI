using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace UserWebAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Email { get; set; } = null!;
        public string HashPassword { get; set; } = null!;
        public string? Username { get; set; }
        public string? FName { get; set; }
        public string? LName { get; set; }

        public int RegisteredObjects { get; set; } = 0;
    }
}
