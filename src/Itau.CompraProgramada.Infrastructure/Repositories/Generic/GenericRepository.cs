using System.Linq.Expressions;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Generic;
using Itau.CompraProgramada.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Itau.CompraProgramada.Infrastructure.Repositories.Generic
{
    public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : Entity
    {
        protected readonly ApplicationDbContext _context = context;
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        public virtual async Task<T?> GetByIdAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync<TKey>(
            int skip, 
            int take, 
            Expression<Func<T, TKey>> orderBy, 
            bool descending = true,
            Expression<Func<T, bool>>? predicate = null)
        {
            var query = predicate != null ? _dbSet.Where(predicate) : _dbSet;
            
            var totalCount = await query.CountAsync();
            
            var orderedQuery = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            
            var items = await orderedQuery
                .Skip(skip)
                .Take(take)
                .ToListAsync();
                
            return (items, totalCount);
        }
    }
}
