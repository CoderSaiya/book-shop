namespace BookShop.Application.DTOs.Res;

public record OrderDetailRes(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    string ShippingAddress,
    string ShippingCity,
    string ShippingPostalCode,
    string ShippingPhone,
    string? Notes,
    DateTime CreatedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? PaidAt,
    IReadOnlyList<OrderItemDto> Items);