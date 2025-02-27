namespace CleanArch.Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
    }

}
