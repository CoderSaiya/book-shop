using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class CategoryRepository(AppDbContext context) : GenericRepository<Category>(context), ICategoryRepository
{
    private readonly AppDbContext _context = context;
    
    public Task<Category?> GetByNameAsync(string name) =>
        _context.Categories
            .FirstOrDefaultAsync(c => c.Name == name);

    public Task<bool> ExistsByNameAsync(string name) =>
        _context.Categories
            .AnyAsync(c => c.Name == name);

    public async Task<IReadOnlyList<Category>> GetAllWithBookCountAsync() =>
        await _context.Categories
            .Include(c => c.Books)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task UpdateIconAsync(Guid categoryId, string? icon)
    {
        var cat = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        if (cat is null) return;

        cat.Icon = icon;
        cat.UpdatedAt = DateTime.UtcNow;
    }
}