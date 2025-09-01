using BookShop.Domain.ValueObjects;

namespace BookShop.Application.DTOs.Req;

public record CreateCouponReq(
    string Code,
    CouponType Type,
    decimal Value,
    decimal? MaxDiscountAmount,
    decimal? MinSubtotal,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    bool IsActive = true,
    Guid? UserId = null);