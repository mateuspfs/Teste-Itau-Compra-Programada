using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class CotacaoRepository(ApplicationDbContext db) : GenericRepository<Cotacao>(db), ICotacaoRepository
    {
        public async Task<Cotacao?> GetUltimaCotacaoAsync(string ticker)
        {
            return await _dbSet
                .Where(c => c.Ticker == ticker)
                .OrderByDescending(c => c.DataPregao)
                .FirstOrDefaultAsync();
        }
    }
}
