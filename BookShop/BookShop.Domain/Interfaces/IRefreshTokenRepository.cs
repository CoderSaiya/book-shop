using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}