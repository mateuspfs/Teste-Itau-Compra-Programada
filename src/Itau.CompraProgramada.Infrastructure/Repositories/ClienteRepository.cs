using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Infrastructure.Data;
using Itau.CompraProgramada.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace Itau.CompraProgramada.Infrastructure.Repositories
{
    public class ClienteRepository(ApplicationDbContext db) : GenericRepository<Cliente>(db), IClienteRepository
    {
        public async Task<Cliente?> GetByCpfAsync(string cpf)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CPF == cpf);
        }

        public async Task<IEnumerable<Cliente>> GetAtivosAsync()
        {
            return await _dbSet.Where(c => c.Ativo).ToListAsync();
        }
    }
}
