
namespace BookShop.Application.DTOs.Req;

public record CreateOrderReq(
    string ShippingAddress,
    string ShippingCity,
    string ShippingPostalCode,
    string ShippingPhone,
    string? Notes,
    string PaymentMethod,
    IReadOnlyList<CreateOrderItemDto> Items);