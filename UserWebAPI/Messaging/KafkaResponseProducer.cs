using Confluent.Kafka;
using MongoDB.Bson;
using System.Text.Json;
using UserWebAPI.Models.Kafka;

namespace UserWebAPI.Messaging
{
    public class KafkaResponseProducer : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private const string ResponseTopic = "car-confirmation-responses";

        public KafkaResponseProducer(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task SendResponseAsync(string objId, string userId, string status, DateTime confirmedAt)
        {
            var response = new ConfirmationResponse
            {
                ObjectId = objId,
                UserId = userId,
                Status = status,
                ConfirmedAt = confirmedAt
            };

            var message = new Message<string, string>
            {
                Key = objId,
                Value = JsonSerializer.Serialize(response)
            };
            var result = await _producer.ProduceAsync(ResponseTopic, message);

        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
