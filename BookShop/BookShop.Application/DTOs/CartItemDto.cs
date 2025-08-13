namespace BookShop.Application.DTOs;

public record CartItemDto(
    Guid BookId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);