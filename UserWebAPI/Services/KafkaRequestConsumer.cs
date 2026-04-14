using Confluent.Kafka;
using System.Text.Json;
using UserWebAPI.Models.Kafka;

namespace UserWebAPI.Services
{
    public class KafkaRequestConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaResponseProducer _producer;
        public const string RequestTopic = "car-confirmation-requests";

        public KafkaRequestConsumer(IServiceScopeFactory scopeFactory, KafkaResponseProducer producer, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _producer = producer;
            var bootstrapServers = configuration["Kafka:BootstrapServers"];

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "users-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();

        }

        private async Task<(string Status, DateTime ConfirmedAt)> ProcessRequestAsync(ConfirmationRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UsersService>();
            var user = await userService.GetAsync(request.UserId);
            if (user != null)
            {
                user.RegisteredObjects++;
                await userService.UpdateAsync(request.UserId, user);
                return ("Confirmed", DateTime.UtcNow);
            }
            else
            {
                return ("Rejected", DateTime.UtcNow);
            }
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(RequestTopic);
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(cancellationToken);
                        var request = JsonSerializer.Deserialize<ConfirmationRequest>(result.Message.Value);
                        if (request != null)
                        {
                            var (status, confirmedAt) = await ProcessRequestAsync(request);
                            await _producer.SendResponseAsync(request.ObjectId, request.UserId, status, confirmedAt);
                            _consumer.Commit(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            });
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
