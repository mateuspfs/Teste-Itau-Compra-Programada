using System.Collections.Generic;
using System.Threading.Tasks;
using Itau.CompraProgramada.Domain.Interfaces.Generic;
using Itau.CompraProgramada.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Itau.CompraProgramada.Infrastructure.Repositories.Generic
{
    public class GenericRepository<TEntity>(CompraProgramadaDbContext db) : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly CompraProgramadaDbContext Db = db;
        protected readonly DbSet<TEntity> DbSet = db.Set<TEntity>();

        public virtual async Task<TEntity> GetByIdAsync(long id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Db.SaveChangesAsync();
        }
    }
}
