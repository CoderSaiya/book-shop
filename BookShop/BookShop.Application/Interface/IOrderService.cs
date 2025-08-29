using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Domain.Entities;

namespace BookShop.Application.Interface;

public interface IOrderService
{
    Task<OrderDetailRes> CreateAsync(Guid userId, CreateOrderReq req);
    Task<OrderDetailRes> GetDetailAsync(Guid orderId);
    Task<IReadOnlyList<OrderDetailRes>> GetByUserAsync(Guid userId, int page, int pageSize);
    Task UpdateStatusAsync(Guid orderId, OrderStatus status);
    Task UpdatePaymentAsync(Guid orderId, PaymentStatus status, DateTime? paidAt);
    Task<decimal> GetRevenueAsync(DateTime fromUtc, DateTime toUtc);
}