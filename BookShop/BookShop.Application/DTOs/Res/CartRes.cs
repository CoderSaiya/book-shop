namespace BookShop.Application.DTOs.Res;

public record CartRes(
    Guid UserId,
    bool IsActive,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<CartItemDto> Items);