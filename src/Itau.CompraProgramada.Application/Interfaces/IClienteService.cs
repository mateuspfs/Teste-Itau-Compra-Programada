using System.Collections.Generic;
using System.Threading.Tasks;
using Itau.CompraProgramada.Application.Common;
using Itau.CompraProgramada.Application.DTOs.Clientes;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface IClienteService
    {
        Task<Result<AdesaoClienteResponse>> AderirAoProdutoAsync(AdesaoClienteRequest request);
        Task<Result<SaidaProdutoResponse>> SairDoProdutoAsync(long clienteId);
        Task<Result<AlterarValorMensalResponse>> AlterarValorMensalAsync(long clienteId, AlterarValorMensalRequest request);
        Task<Result<RentabilidadeResponse>> ObterRentabilidadeAsync(long clienteId);
        Task<Result<ResultadoPaginado<AdesaoClienteResponse>>> ObterTodosPaginaAsync(int pagina, int tamanhoPagina, bool ordemDesc = true);
        Task<Result<ClienteResumoResponse>> ObterResumoDashboardAsync();
    }
}
