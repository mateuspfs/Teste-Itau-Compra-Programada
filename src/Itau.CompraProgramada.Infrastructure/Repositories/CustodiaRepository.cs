using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class CustodiaRepository(ApplicationDbContext db) : GenericRepository<Custodia>(db), ICustodiaRepository
    {
        public async Task<IEnumerable<Custodia>> GetByContaGraficaIdAsync(long contaGraficaId)
        {
            return await db.Custodias.Where(c => c.ContaGraficaId == contaGraficaId).ToListAsync();
        }
    }
}
