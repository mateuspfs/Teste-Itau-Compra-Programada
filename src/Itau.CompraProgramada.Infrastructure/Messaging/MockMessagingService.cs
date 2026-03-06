using System.Text.Json;
using Itau.CompraProgramada.Domain.Interfaces.Messaging;
using Microsoft.Extensions.Logging;

namespace Itau.CompraProgramada.Infrastructure.Messaging
{
    public class MockMessagingService(ILogger<MockMessagingService> logger) : IMessagingService
    {
        public Task PublishAsync<T>(string topic, T message)
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });
            logger.LogInformation("PUBLISHED TO KAFKA TOPIC [{Topic}]: {Message}", topic, json);
            return Task.CompletedTask;
        }
    }
}
