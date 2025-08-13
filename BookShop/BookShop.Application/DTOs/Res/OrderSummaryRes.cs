
namespace BookShop.Application.DTOs.Res;

public record OrderSummaryRes(
    Guid Id,
    string OrderNumber,
    decimal TotalAmount,
    string Status,
    string PaymentStatus,
    DateTime CreatedAt);