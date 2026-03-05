using System.Collections.Generic;
using System.Threading.Tasks;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Generic;

namespace Itau.CompraProgramada.Domain.Interfaces.Respositories
{
    public interface ICestaRecomendacaoRepository : IGenericRepository<CestaRecomendacao>
    {
        Task<CestaRecomendacao?> GetAtivaAsync();
        Task<IEnumerable<CestaRecomendacao>> GetHistoricoAsync();
    }
}
