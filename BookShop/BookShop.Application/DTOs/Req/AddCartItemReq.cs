namespace BookShop.Application.DTOs.Req;

public record AddCartItemReq(
    Guid BookId,
    int Quantity,
    decimal UnitPrice);