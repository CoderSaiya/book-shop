using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class RefreshTokenRepository(AppDbContext context) : GenericRepository<RefreshToken>(context)
{
    
}