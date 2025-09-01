using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class CouponRepository(AppDbContext context) : GenericRepository<Coupon>(context), ICouponRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Coupon?> GetByUserAndCodeAsync(Guid userId, string code, bool tracking = false)
    {
        var q = _context.Coupons.Where(c => c.UserId == userId && c.Code == code);
        if (!tracking) q = q.AsNoTracking();
        return await q.FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<Coupon>> GetByUserAsync(Guid userId, bool includeUsed = true, bool includeInactive = false)
    {
        var q = _context.Coupons.Where(x => x.UserId == userId || x.UserId == null);

        if (!includeUsed) q = q.Where(x => !x.IsUsed);
        if (!includeInactive) q = q.Where(x => x.IsActive);
        
        return await q
            .OrderByDescending(x => x.UserId == userId)
            .ThenByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<Coupon?> GetByCodeAsync(string code, bool tracking = false) =>
        await _context.Coupons
            .FirstOrDefaultAsync(x => x.Code == code);
}