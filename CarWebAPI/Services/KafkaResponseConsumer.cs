using CarWebAPI.Models.Kafka;
using Confluent.Kafka;
using System.Text.Json;

namespace CarWebAPI.Services
{
    public class KafkaResponseConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        public const string ResponseTopic = "car-confirmation-responses";

        public KafkaResponseConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            var bootstrapServers = configuration["Kafka:BootstrapServers"];

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "cars-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();

        }

        private async Task ProcessResponseAsync(ConfirmationResponse response)
        {
            using var scope = _scopeFactory.CreateScope();
            var carService = scope.ServiceProvider.GetRequiredService<CarsService>();
            var obj = await carService.GetAsync(response.ObjectId);
            if (obj != null)
            {
                obj.ConfirmationStatus = response.Status;
                obj.ConfirmedBy = response.UserId;
                obj.ConfirmedAt = response.ConfirmedAt;
                await carService.UpdateAsync(response.ObjectId, obj);
                await carService.InvalidateListCacheAsync(); 
            }
            
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(ResponseTopic);
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(cancellationToken);
                        var response = JsonSerializer.Deserialize<ConfirmationResponse>(result.Message.Value);
                        if (response != null)
                        {
                            await ProcessResponseAsync(response);
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
