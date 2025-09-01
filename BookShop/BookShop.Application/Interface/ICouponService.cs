using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface ICouponService
{
    Task<CouponRes> GrantAsync(CreateCouponReq req);
    Task<IReadOnlyList<CouponRes>> ListMineAsync(Guid userId, bool includeUsed = true);
    Task<ValidateCouponRes> ValidateAsync(Guid userId, ValidateCouponReq req);
    Task UseAsync(Guid userId, string code, string? context = null);
    Task<IReadOnlyList<EligibleCouponRes>> ListEligibleAsync(Guid userId, decimal subtotal);
}