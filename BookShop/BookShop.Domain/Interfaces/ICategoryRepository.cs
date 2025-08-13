using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
    Task<IReadOnlyList<Category>> GetAllWithBookCountAsync();
    Task UpdateIconAsync(Guid categoryId, string? icon);
}