using BookShop.Domain.ValueObjects;

namespace BookShop.Application.DTOs.Req;

public record ConfirmPaymentReq(
    PaymentProvider Provider,
    string PaymentOrderId,
    Guid ShopOrderId);