using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class ItemCestaRepository(ApplicationDbContext db) : GenericRepository<ItemCesta>(db), IItemCestaRepository
    {
    }
}
