using System.Threading.Tasks;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface IMotorCompraEngine
    {
        Task ExecutarProcessamentoDiarioAsync(DateTime dataProcessamento);
    }
}
