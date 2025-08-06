using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email.Equals(email));

    public async Task<User?> GetByIdWithProfileAsync(Guid id) =>
        await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id.Equals(id)); 

    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Users
            .AnyAsync(u => u.Email.Equals(email));
}