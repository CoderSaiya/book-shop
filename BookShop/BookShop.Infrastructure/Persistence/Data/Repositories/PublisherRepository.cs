using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class PublisherRepository(AppDbContext context) : GenericRepository<Publisher>(context), IPublisherRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Publisher?> GetWithBookAsync(Guid publisherId) =>
        await _context.Publishers
            .Include(p => p.Books)
            .FirstOrDefaultAsync(p => p.Id == publisherId);

    public override async Task<IEnumerable<Publisher>> ListAsync() =>
        await _context.Publishers
            .Include(p => p.Books)
            .ToListAsync();
}