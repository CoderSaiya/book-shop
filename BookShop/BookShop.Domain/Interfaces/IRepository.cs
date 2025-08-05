using System.Linq.Expressions;

namespace BookShop.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> ListAsync();
    Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> ListAsync<TFilter>(TFilter filter)
        where TFilter : class;
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Guid id);
}