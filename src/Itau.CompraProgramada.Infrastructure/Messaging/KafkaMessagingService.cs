using System.Text.Json;
using Confluent.Kafka;
using Itau.CompraProgramada.Domain.Interfaces.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Itau.CompraProgramada.Infrastructure.Messaging
{
    public class KafkaMessagingService : IMessagingService, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaMessagingService> _logger;

        public KafkaMessagingService(IConfiguration configuration, ILogger<KafkaMessagingService> logger)
        {
            _logger = logger;
            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task PublishAsync<T>(string topic, T message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var kafkaMessage = new Message<Null, string> { Value = json };

                var result = await _producer.ProduceAsync(topic, kafkaMessage);
                
                _logger.LogInformation("PUBLISHED TO KAFKA TOPIC [{Topic}]: {Status}", topic, result.Status);
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError(e, "FAILED TO PUBLISH TO KAFKA TOPIC [{Topic}]", topic);
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
