namespace BookShop.Application.DTOs;

public record CreateOrderItemDto(
    Guid BookId, 
    int Quantity, 
    decimal UnitPrice);