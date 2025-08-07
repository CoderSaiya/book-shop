using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithProfileAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
}