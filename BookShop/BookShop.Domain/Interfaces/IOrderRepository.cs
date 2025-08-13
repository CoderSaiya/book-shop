using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, bool includeItems = true);
    Task<IReadOnlyList<Order>> GetByUserAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, int page = 1, int pageSize = 50);

    Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus);
    Task UpdatePaymentAsync(Guid orderId, PaymentStatus paymentStatus, DateTime? paidAt);

    Task<decimal> GetRevenueAsync(DateTime fromUtc, DateTime toUtc);
}