using BookShop.Domain.ValueObjects;

namespace BookShop.Application.DTOs.Req;

public record PaymentReq(
    double Amount,
    PaymentProvider Provider,
    string? OrderInfo = null,
    string? ClientIp = null // Cho VNPay
    );