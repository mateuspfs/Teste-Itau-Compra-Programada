using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class OrdemCompraRepository(CompraProgramadaDbContext db) : GenericRepository<OrdemCompra>(db), IOrdemCompraRepository
    {
    }
}
