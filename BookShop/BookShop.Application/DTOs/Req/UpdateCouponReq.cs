namespace BookShop.Application.DTOs.Req;

public record UpdateCouponReq(
    string? Title,
    string? Type,
    decimal? Value,
    decimal? MaxDiscountAmount,
    decimal? MinOrderAmount,
    bool? FreeShipping,
    int? MaxUsage,
    int? MaxUsagePerUser,
    DateTime? StartsAtUtc,
    DateTime? ExpiresAtUtc,
    bool? IsActive);