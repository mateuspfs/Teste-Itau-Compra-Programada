using System.Threading.Tasks;
using Itau.CompraProgramada.Application.DTOs.Clientes;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface IClienteService
    {
        Task<AdesaoClienteResponse> AderirAoProdutoAsync(AdesaoClienteRequest request);
        Task<SaidaProdutoResponse> SairDoProdutoAsync(long clienteId);
        Task<AlterarValorMensalResponse> AlterarValorMensalAsync(long clienteId, AlterarValorMensalRequest request);
    }
}
