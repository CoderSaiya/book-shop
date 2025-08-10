using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<User>> SearchAsync(string keyword, int page = 1, int pageSize = 50)
    {
        var query = _context.Users
            .Include(u => u.Profile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = $"%{keyword.Trim()}%";
            query = query.Where(u =>
                EF.Functions.Like(u.Email.Address, term) ||
                (u.Profile.Name != null && EF.Functions.Like(u.Profile.Name.FirstName, term)) ||
                (u.Profile.Name != null && EF.Functions.Like(u.Profile.Name.LastName, term)) ||
                (u.Profile.Phone != null && EF.Functions.Like(u.Profile.Phone.SubscriberNumber, term))
            );
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email.Address == email);

    public async Task<User?> GetByIdWithProfileAsync(Guid id) =>
        await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id.Equals(id)); 

    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Users
            .AnyAsync(u => u.Email.Address == email);
}