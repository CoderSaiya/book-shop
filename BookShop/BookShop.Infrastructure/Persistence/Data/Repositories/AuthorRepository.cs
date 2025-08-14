using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class AuthorRepository(AppDbContext context) : GenericRepository<Author>(context), IAuthorRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Author?> GetWithBookAsync(Guid authorId) => 
        await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == authorId);

    public override async Task<IEnumerable<Author>> ListAsync() =>
        await _context.Authors
            .Include(a => a.Books)
            .ThenInclude(b => b.Publisher)
            .Include(a => a.Books)
            .ThenInclude(b => b.Category)
            .ToListAsync();
}