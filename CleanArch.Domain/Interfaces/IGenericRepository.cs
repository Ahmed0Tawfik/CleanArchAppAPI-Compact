namespace CleanArch.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(string? Search, int pageNumber, int pageSize, bool? inStock, bool? isNew);
    }

}
