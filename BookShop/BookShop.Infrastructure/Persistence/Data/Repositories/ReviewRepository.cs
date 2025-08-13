using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class ReviewRepository(AppDbContext context) : GenericRepository<Review>(context), IReviewRepository
{
    private readonly AppDbContext _context = context;
    
    public async Task<bool> ExistsByUserAndBookAsync(Guid userId, Guid bookId) =>
        await _context.Reviews
            .AnyAsync(r => r.UserId == userId && r.BookId == bookId);

    public async Task<IReadOnlyList<Review>> GetByBookAsync(Guid bookId, bool onlyVerified = false, int page = 1, int pageSize = 20)
    {
        var q = _context.Reviews.Where(r => r.BookId == bookId);
        if (onlyVerified) q = q.Where(r => r.IsVerifiedPurchase);

        return await q.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingAsync(Guid bookId) =>
        await _context.Reviews.Where(r => r.BookId == bookId)
            .Select(r => (double)r.Rating)
            .DefaultIfEmpty(0)
            .AverageAsync();

    public async Task IncrementHelpfulAsync(Guid reviewId)
    {
        await _context.Reviews
            .Where(r => r.Id == reviewId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.HelpfulCount, r => r.HelpfulCount + 1));
    }

    public Task<Review?> GetUserReviewAsync(Guid userId, Guid bookId) =>
        _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);
}