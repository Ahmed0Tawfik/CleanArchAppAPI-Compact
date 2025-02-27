using CleanArch.Domain.Interfaces;
using CleanArch.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (!_repositories.ContainsKey(typeof(T)))
            {
                _repositories[typeof(T)] = new GenericRepository<T>(_context);
            }
            return (IGenericRepository<T>)_repositories[typeof(T)];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
