namespace BookShop.Application.DTOs.Res;

public record ReviewRes(
    Guid Id,
    Guid UserId,
    Guid BookId,
    int Rating,
    string? Comment,
    bool IsVerifiedPurchase,
    int HelpfulCount,
    DateTime CreatedAt);