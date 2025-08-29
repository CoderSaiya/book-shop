using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class OrderRepository(AppDbContext context) : GenericRepository<Order>(context), IOrderRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, bool includeItems = true)
    {
        IQueryable<Order> q = _context.Orders.AsQueryable();
        if (includeItems) 
            q = q.Include(o => o.OrderItems);
        
        return await q
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public override async Task<Order?> GetByIdAsync(Guid id) =>
        await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(ot => ot.Book)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IReadOnlyList<Order>> GetByUserAsync(Guid userId, int page = 1, int pageSize = 20) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(ot => ot.Book)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, int page = 1, int pageSize = 50) =>
        await _context.Orders
            .Where(o => o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return;
        
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        if (newStatus == OrderStatus.Shipped) order.ShippedAt = DateTime.UtcNow;
        if (newStatus == OrderStatus.Delivered) order.DeliveredAt = DateTime.UtcNow;
    }

    public async Task UpdatePaymentAsync(Guid orderId, PaymentStatus paymentStatus, DateTime? paidAt)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return;

        order.PaymentStatus = paymentStatus;
        order.PaidAt = paidAt;
        order.UpdatedAt = DateTime.UtcNow;
        
        if (paymentStatus == PaymentStatus.Paid && order.Status == OrderStatus.Pending)
            order.Status = OrderStatus.Confirmed;
    }

    public async Task<decimal> GetRevenueAsync(DateTime fromUtc, DateTime toUtc) =>
        await _context.Orders
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc && o.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(o => o.TotalAmount);
}