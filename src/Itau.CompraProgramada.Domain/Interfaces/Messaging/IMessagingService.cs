using System.Threading.Tasks;

namespace Itau.CompraProgramada.Domain.Interfaces.Messaging
{
    public interface IMessagingService
    {
        Task PublishAsync<T>(string topic, T message);
    }
}
