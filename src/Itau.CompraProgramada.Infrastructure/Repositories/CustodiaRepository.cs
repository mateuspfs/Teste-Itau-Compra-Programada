using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class CustodiaRepository(CompraProgramadaDbContext db) : GenericRepository<Custodia>(db), ICustodiaRepository
    {
    }
}
