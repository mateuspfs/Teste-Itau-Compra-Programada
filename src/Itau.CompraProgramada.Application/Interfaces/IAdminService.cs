using System.Collections.Generic;
using System.Threading.Tasks;
using Itau.CompraProgramada.Application.Common;
using Itau.CompraProgramada.Application.DTOs.Admin;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface IAdminService
    {
        Task<Result<CestaCadastroResponse>> CadastrarAlterarCestaAsync(CestaRequest request);
        Task<Result<CestaDetalhesResponse>> ObterCestaAtualAsync();
        Task<Result<IEnumerable<CestaHistoricoResponse>>> ObterHistoricoCestasAsync();
        Task<Result<CustodiaMasterResponse>> ObterCustodiaMasterAsync();
    }
}
