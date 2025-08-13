namespace BookShop.Application.DTOs;

public record OrderItemDto(
    Guid BookId,
    string? BookTitle,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);