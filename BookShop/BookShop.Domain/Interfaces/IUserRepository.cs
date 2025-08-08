using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<User>> SearchAsync(string keyword, int page = 1, int pageSize = 50);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithProfileAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
}