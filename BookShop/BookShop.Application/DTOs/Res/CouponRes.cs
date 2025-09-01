namespace BookShop.Application.DTOs.Res;

public record CouponRes(
    Guid Id,
    string Code,
    string Type,
    decimal Value,
    decimal? MaxDiscountAmount,
    decimal? MinSubtotal,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    bool IsUsed,
    DateTime? UsedAt,
    bool IsActive,
    DateTime CreatedAt);