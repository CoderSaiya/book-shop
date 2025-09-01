using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByUserAndCodeAsync(Guid userId, string code, bool tracking = false);
    Task<IReadOnlyList<Coupon>> GetByUserAsync(Guid userId, bool includeUsed = true, bool includeInactive = false);
    Task<Coupon?> GetByCodeAsync(string code, bool tracking = false);
}