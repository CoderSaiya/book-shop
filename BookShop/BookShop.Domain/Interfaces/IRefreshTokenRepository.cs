using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}