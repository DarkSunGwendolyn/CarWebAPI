using CarWebAPI.Models.Kafka;
using Confluent.Kafka;
using System.Text.Json;


namespace CarWebAPI.Services
{
    public class KafkaRequestProducer : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private const string Topic = "car-confirmation-requests";

        public KafkaRequestProducer(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task SendRequestAsync(string carId, string userId)
        {
            var request = new ConfirmationRequest
            { 
                ObjectId = carId,
                UserId = userId,
                RequestTime = DateTime.UtcNow,
            };

            var msg = new Message<string, string>
            {
                Key = carId,
                Value = JsonSerializer.Serialize(request)
            };
            
            var result = await _producer.ProduceAsync(Topic, msg);
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
