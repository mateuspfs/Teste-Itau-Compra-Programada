using System.Collections.Generic;
using System.Threading.Tasks;
using Itau.CompraProgramada.Application.DTOs.Admin;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface IAdminService
    {
        Task<CestaCadastroResponse> CadastrarAlterarCestaAsync(CestaRequest request);
        Task<CestaDetalhesResponse> ObterCestaAtualAsync();
        Task<IEnumerable<CestaHistoricoResponse>> ObterHistoricoCestasAsync();
        Task<CustodiaMasterResponse> ObterCustodiaMasterAsync();
    }
}
