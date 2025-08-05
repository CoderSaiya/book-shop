using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithProfileAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
}