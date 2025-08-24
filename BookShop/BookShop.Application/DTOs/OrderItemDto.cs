namespace BookShop.Application.DTOs;

public record OrderItemDto(
    Guid BookId,
    string CoverImage,
    string? BookTitle,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);