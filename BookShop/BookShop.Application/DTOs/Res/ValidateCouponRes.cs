namespace BookShop.Application.DTOs.Res;

public record ValidateCouponRes(
    bool IsValid,
    string Message,
    decimal DiscountAmount);