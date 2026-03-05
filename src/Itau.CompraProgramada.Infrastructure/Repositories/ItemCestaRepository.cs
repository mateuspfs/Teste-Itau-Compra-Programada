using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class ItemCestaRepository(ApplicationDbContext db) : GenericRepository<ItemCesta>(db), IItemCestaRepository
    {
        public async Task<IEnumerable<ItemCesta>> GetByCestaIdAsync(long cestaId)
        {
            return await _dbSet.Where(i => i.CestaId == cestaId).ToListAsync();
        }
    }
}
