using BookShop.Application.DTOs;
using FluentValidation;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;
using BookShop.Domain.Models;

namespace BookShop.Application.Services;

public class OrderService(
    IUnitOfWork uow,
    IMailSender mail
    ) : IOrderService
{
    public async Task<OrderDetailRes> CreateAsync(Guid userId, CreateOrderReq req)
    {
        if (req.Items is null || req.Items.Count == 0)
            throw new ValidationException("Đơn hàng phải có ít nhất 1 sản phẩm.");
        
        var user = await uow.Users.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User", userId.ToString());

        // Tính tổng tiền từ client gửi
        var total = req.Items.Sum(i => i.UnitPrice * Math.Max(1, i.Quantity));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            UserId = userId,
            TotalAmount = total,
            Status = OrderStatus.Pending,
            PaymentMethod = PaymentMethodHelper.ParseOrThrow(req.PaymentMethod),
            PaymentStatus = PaymentStatus.Pending,
            ShippingAddress = req.ShippingAddress,
            ShippingCity = req.ShippingCity,
            ShippingPostalCode = req.ShippingPostalCode,
            ShippingPhone = req.ShippingPhone,
            Notes = req.Notes
        };

        foreach (var i in req.Items)
        {
            order.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                BookId = i.BookId,
                Quantity = Math.Max(1, i.Quantity),
                UnitPrice = i.UnitPrice,
                TotalPrice = i.UnitPrice * Math.Max(1, i.Quantity)
            });
        }

        await uow.Orders.AddAsync(order);

        // Deactivate cart sau khi đặt hàng
        var cart = await uow.Carts.GetActiveCartByUserAsync(userId, includeItems: true);
        if (cart is not null) await uow.Carts.DeactivateAsync(cart.UserId);

        // Email xác nhận
        var mm = new EmailMessage
        {
            ToEmail = user.Email.ToString(),
            Subject = $"Xác nhận đơn hàng {order.OrderNumber}",
            Body = $"""
                        <p>Chào bạn,</p>
                        <p>Đơn hàng <b>{order.OrderNumber}</b> đã được tạo với tổng tiền <b>{order.TotalAmount:C}</b>.</p>
                        <p>Trạng thái: {order.Status}, Thanh toán: {order.PaymentStatus}</p>
                        <p>Địa chỉ giao: {order.ShippingAddress}, {order.ShippingCity}</p>
                    """
        };
        _ = mail.SendEmailAsync(mm);

        return await MapDetailAsync(order.Id);
    }

    public async Task<OrderDetailRes> GetDetailAsync(Guid orderId)
    {
        var o = await uow.Orders.GetByIdAsync(orderId) 
                ?? throw new NotFoundException("Order", orderId.ToString());
        return Map(o);
    }

    public async Task<IReadOnlyList<OrderSummaryRes>> GetByUserAsync(Guid userId, int page, int pageSize)
    {
        var list = await uow.Orders.GetByUserAsync(userId, page, pageSize);
        return list.Select(o => new OrderSummaryRes(
            o.Id,
            o.OrderNumber,
            o.TotalAmount,
            o.Status.ToString(),
            o.PaymentStatus.ToString(),
            o.CreatedAt)
        ).ToList();
    }

    public async Task UpdateStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await uow.Orders.GetByIdAsync(orderId);
        if (order is null) 
            throw new NotFoundException("Order", orderId.ToString());
        
        await uow.Orders.UpdateStatusAsync(orderId, status);
        await uow.SaveAsync();
        
        if (status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            var subject = status switch
            {
                OrderStatus.Shipped => $"Đơn hàng {order.OrderNumber} đã được gửi",
                OrderStatus.Delivered => $"Đơn hàng {order.OrderNumber} đã giao thành công",
                OrderStatus.Cancelled => $"Đơn hàng {order.OrderNumber} đã bị huỷ",
                _ => $"Cập nhật đơn hàng {order.OrderNumber}"
            };

            var mm = new EmailMessage
            {
                ToEmail = order.User.Email.ToString(),
                Subject = subject,
                Body = $"<p>Đơn hàng <b>{order.OrderNumber}</b> hiện ở trạng thái <b>{status}</b>.</p>"
            };
            _ = mail.SendEmailAsync(mm);
        }
    }

    public async Task UpdatePaymentAsync(Guid orderId, PaymentStatus status, DateTime? paidAt)
    {
        var order = await uow.Orders.GetByIdAsync(orderId);
        if (order is null) 
            throw new NotFoundException("Order", orderId.ToString());
        
        await uow.Orders.UpdatePaymentAsync(orderId, status, paidAt);
        await uow.SaveAsync();

        if (status == PaymentStatus.Paid)
        {
            var mm = new EmailMessage
            {
                ToEmail = order.User.Email.ToString(),
                Subject = $"Đã nhận thanh toán cho {order.OrderNumber}",
                Body = $"<p>Chúng tôi đã nhận thanh toán cho đơn hàng <b>{order.OrderNumber}</b>. Xin cảm ơn!</p>"
            };
            _ = mail.SendEmailAsync(mm);
        }
    }

    public Task<decimal> GetRevenueAsync(DateTime fromUtc, DateTime toUtc) => 
        uow.Orders.GetRevenueAsync(fromUtc, toUtc);
    
    private async Task<OrderDetailRes> MapDetailAsync(Guid id)
        => Map((await uow.Orders.GetByIdAsync(id))!);
    
    private static OrderDetailRes Map(Order o)
    {
        var items = o.OrderItems.Select(i => new OrderItemDto(i.BookId, i.Book?.Title, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList();
        return new OrderDetailRes(
            o.Id,
            o.OrderNumber,
            o.UserId,
            o.TotalAmount,
            o.Status.ToString(),
            o.PaymentStatus.ToString(),
            o.ShippingAddress,
            o.ShippingCity,
            o.ShippingPostalCode,
            o.ShippingPhone,
            o.Notes,
            o.CreatedAt,
            o.ShippedAt,
            o.DeliveredAt,
            o.PaidAt,
            items);
    }
}