using CleanArch.Domain;
using CleanArch.Domain.Interfaces;
using CleanArch.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return entity;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
                    string? search,
                    int pageNumber,
                    int pageSize,
                    bool? inStock = true,
                    bool? isNew = true)
        {
            // Start with the base query
            var query = _dbSet.AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                // Dynamically filter by "Name" property (assuming entities have a "Name" property)
                query = query.Where(RepositoryHelper.GenerateNameFilterExpression<T>(search));
            }

            // Apply in-stock filter if provided
            if (inStock.HasValue)
            {
                // Dynamically filter by "InStock" property (assuming entities have an "InStock" property)
                query = query.Where(RepositoryHelper.GenerateInStockFilterExpression<T>(inStock.Value));
            }

            if(isNew.HasValue)
            {
                query = query.Where(RepositoryHelper.GenerateIsNewFilterExpression<T>(isNew.Value));
            }

            // Get total count after applying filters
            var totalCount = await query.CountAsync();

            // Apply pagination
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
