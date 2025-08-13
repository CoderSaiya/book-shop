using BookShop.Domain.Entities;

namespace BookShop.Application.DTOs.Req;

public record UpdatePaymentReq(
    PaymentStatus PaymentStatus,
    DateTime? PaidAt);