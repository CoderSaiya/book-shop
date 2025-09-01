namespace BookShop.Application.DTOs.Res;

public record EligibleCouponRes(
    Guid Id,
    string Code,
    string Type,
    decimal Value,
    decimal? MaxDiscountAmount,
    decimal? MinSubtotal,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    decimal Discount,
    string Message);