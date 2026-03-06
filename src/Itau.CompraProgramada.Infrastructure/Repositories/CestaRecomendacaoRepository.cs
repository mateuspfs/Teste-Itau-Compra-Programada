using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class CestaRecomendacaoRepository(ApplicationDbContext db) : GenericRepository<CestaRecomendacao>(db), ICestaRecomendacaoRepository
    {

        public async Task<IEnumerable<CestaRecomendacao>> GetHistoricoAsync()
        {
            return await _dbSet.OrderByDescending(c => c.DataCriacao).ToListAsync();
        }
    }
}
