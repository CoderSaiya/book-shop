using System.Globalization;
using System.Net;
using System.Text;
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
using BookShop.Domain.ValueObjects;

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
        var subTotal = req.Items.Sum(i => i.UnitPrice * Math.Max(1, i.Quantity));
        var ship = subTotal >= 300000 ? 0M : 25000M;
        var tax = subTotal * 0.08M;
        var total = subTotal + ship + tax;

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
            var book = await uow.Books.GetByIdAsync(i.BookId);
            if (book is null) 
                throw new NotFoundException("Book", i.BookId.ToString());

            if (book.Stock <= 0 || book.Stock < i.Quantity)
                throw new Exception("Vượt quá số lượng cho phép");
            
            book.Stock -= i.Quantity;
            
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

        await uow.Carts.ClearAsync(userId);
        
        await uow.SaveAsync();

        // Deactivate cart sau khi đặt hàng
        var cart = await uow.Carts.GetActiveCartByUserAsync(userId, includeItems: true);
        if (cart is not null) await uow.Carts.DeactivateAsync(cart.UserId);
        
        // Email xác nhận
        var mm = new EmailMessage
        {
            ToEmail = user.Email.ToString(),
            Subject = $"Xác nhận đơn hàng #{order.OrderNumber}",
            Body = $@"""
                     <!DOCTYPE html>
                     <html lang='vi'>
                     <head>
                         <meta charset='UTF-8'>
                         <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                         <title>Xác nhận đơn hàng - BookShop</title>
                     </head>
                     <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                         <table style='width: 100%; max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                             <!-- Header -->
                             <tr>
                                 <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                                     <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold;'>📚 BookShop</h1>
                                     <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Kho sách trực tuyến hàng đầu</p>
                                 </td>
                             </tr>
                             
                             <!-- Main Content -->
                             <tr>
                                 <td style='padding: 40px 30px;'>
                                     <div style='text-align: center; margin-bottom: 30px;'>
                                         <h2 style='color: #333333; margin: 0; font-size: 24px;'>Xác nhận đơn hàng thành công! ✅</h2>
                                     </div>
                                     
                                     <p style='color: #666666; font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                                         Xin chào <strong style='color: #667eea;'>{user.Email}</strong>,
                                     </p>
                                     
                                     <p style='color: #666666; font-size: 16px; line-height: 1.6; margin-bottom: 25px;'>
                                         Cảm ơn bạn đã đặt hàng tại <strong>BookShop</strong>! Đơn hàng của bạn đã được tạo thành công và đang được xử lý.
                                     </p>
                                     
                                     <!-- Order Info -->
                                     <div style='background-color: #f8f9ff; border: 1px solid #e1e5e9; padding: 25px; margin: 25px 0; border-radius: 8px;'>
                                         <h3 style='color: #333333; margin: 0 0 20px 0; font-size: 18px;'>📋 Thông tin đơn hàng</h3>
                                         <table style='width: 100%; border-collapse: collapse;'>
                                             <tr>
                                                 <td style='color: #666666; font-size: 14px; padding: 8px 0; border-bottom: 1px solid #f0f0f0;'><strong>Mã đơn hàng:</strong></td>
                                                 <td style='color: #333333; font-size: 14px; padding: 8px 0; border-bottom: 1px solid #f0f0f0; text-align: right;'><strong style='color: #667eea;'>#{order.OrderNumber}</strong></td>
                                             </tr>
                                             <tr>
                                                 <td style='color: #666666; font-size: 14px; padding: 8px 0; border-bottom: 1px solid #f0f0f0;'><strong>Ngày đặt hàng:</strong></td>
                                                 <td style='color: #333333; font-size: 14px; padding: 8px 0; border-bottom: 1px solid #f0f0f0; text-align: right;'>{order.CreatedAt:dd/MM/yyyy HH:mm}</td>
                                             </tr>
                                             <tr>
                                                 <td style='color: #666666; font-size: 14px; padding: 8px 0; border-bottom: 1px solid #f0f0f0;'><strong>Trạng thái:</strong></td>
                                                 <td style='color: #333333; font-size: 14px; padding: 8px 0; border-bottom: 1px solid #f0f0f0; text-align: right;'>
                                                     <span style='background-color: #fff3cd; color: #856404; padding: 4px 8px; border-radius: 12px; font-size: 12px;'>{order.Status}</span>
                                                 </td>
                                             </tr>
                                             <tr>
                                                 <td style='color: #666666; font-size: 14px; padding: 8px 0; border-bottom: 1px solid #f0f0f0;'><strong>Thanh toán:</strong></td>
                                                 <td style='color: #333333; font-size: 14px; padding: 8px 0; border-bottom: 1px solid #f0f0f0; text-align: right;'>
                                                     <span style='background-color: {(order.PaymentStatus == PaymentStatus.Paid ? "#d4edda" : "#fff3cd")}; color: {(order.PaymentStatus == PaymentStatus.Paid ? "#155724" : "#856404")}; padding: 4px 8px; border-radius: 12px; font-size: 12px;'>{order.PaymentStatus}</span>
                                                 </td>
                                             </tr>
                                             <tr>
                                                 <td style='color: #666666; font-size: 14px; padding: 8px 0;'><strong>Phương thức thanh toán:</strong></td>
                                                 <td style='color: #333333; font-size: 14px; padding: 8px 0; text-align: right;'>{order.PaymentMethod}</td>
                                             </tr>
                                         </table>
                                     </div>
                                     
                                     <!-- Shipping Info -->
                                     <div style='background-color: #f8f9ff; border: 1px solid #e1e5e9; padding: 25px; margin: 25px 0; border-radius: 8px;'>
                                         <h3 style='color: #333333; margin: 0 0 15px 0; font-size: 18px;'>🚚 Địa chỉ giao hàng</h3>
                                         <p style='color: #666666; margin: 5px 0; font-size: 14px; line-height: 1.5;'>
                                             <strong>Địa chỉ:</strong> {order.ShippingAddress}<br>
                                             <strong>Thành phố:</strong> {order.ShippingCity}<br>
                                             <strong>Mã bưu điện:</strong> {order.ShippingPostalCode}<br>
                                             <strong>Số điện thoại:</strong> {order.ShippingPhone}
                                         </p>
                                         {(string.IsNullOrEmpty(order.Notes) ? "" : $"<p style='color: #666666; margin: 10px 0 0 0; font-size: 14px;'><strong>Ghi chú:</strong> {order.Notes}</p>")}
                                     </div>
                                     
                                     <!-- Order Items -->
                                     <div style='background-color: #ffffff; border: 1px solid #e1e5e9; border-radius: 8px; overflow: hidden; margin: 25px 0;'>
                                         <div style='background-color: #667eea; color: white; padding: 15px 20px;'>
                                             <h3 style='margin: 0; font-size: 18px;'>📚 Chi tiết đơn hàng</h3>
                                         </div>
                                         <div style='padding: 0;'>
                                             <table style='width: 100%; border-collapse: collapse;'>
                                                 {string.Join("", order.OrderItems.Select(item => $@"""
                                                 <tr style='border-bottom: 1px solid #f0f0f0;'>
                                                     <td style='padding: 15px; width: 60px;'>
                                                         <img src='cid:image-item' alt='{item.Book.Title}' style='width: 50px; height: 70px; object-fit: cover; border-radius: 4px;'>
                                                     </td>
                                                     <td style='padding: 15px; vertical-align: top;'>
                                                         <h4 style='margin: 0 0 5px 0; font-size: 16px; color: #333333;'>{item.Book.Title}</h4>
                                                         <p style='margin: 0; font-size: 14px; color: #666666;'>Tác giả: {item.Book.Author}</p>
                                                         <p style='margin: 5px 0 0 0; font-size: 14px; color: #666666;'>Số lượng: <strong>{item.Quantity}</strong></p>
                                                     </td>
                                                     <td style='padding: 15px; text-align: right; vertical-align: top;'>
                                                         <p style='margin: 0; font-size: 14px; color: #666666;'>{Vnd(item.UnitPrice)} x {item.Quantity}</p>
                                                         <p style='margin: 5px 0 0 0; font-size: 16px; color: #333333; font-weight: bold;'>{Vnd(item.TotalPrice)}</p>
                                                     </td>
                                                 </tr>
                                                    """
                                                 ))}
                                             </table>
                                         </div>
                                         <div style='background-color: #f8f9ff; padding: 20px; border-top: 2px solid #667eea;'>
                                             <div style='text-align: right;'>
                                                 <p style='margin: 0; font-size: 18px; color: #333333;'><strong>Tổng cộng: <span style='color: #667eea;'>{Vnd(order.TotalAmount)}</span></strong></p>
                                             </div>
                                         </div>
                                     </div>
                                     
                                     <div style='background-color: #e8f5e8; border-left: 4px solid #28a745; padding: 20px; margin: 25px 0; border-radius: 5px;'>
                                         <h3 style='color: #333333; margin: 0 0 15px 0; font-size: 18px;'>✨ Thông tin quan trọng:</h3>
                                         <ul style='color: #666666; margin: 0; padding-left: 20px;'>
                                             <li style='margin-bottom: 8px;'>Đơn hàng sẽ được xử lý trong vòng 1-2 ngày làm việc</li>
                                             <li style='margin-bottom: 8px;'>Bạn sẽ nhận được email thông báo khi đơn hàng được giao</li>
                                             <li style='margin-bottom: 8px;'>Liên hệ hotline nếu cần hỗ trợ: <strong>0935-234-074</strong></li>
                                         </ul>
                                     </div>
                                     
                                     <div style='text-align: center; margin: 30px 0;'>
                                         <a href='#' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block; font-size: 16px; margin-right: 10px;'>
                                             📦 Theo dõi đơn hàng
                                         </a>
                                         <a href='#' style='background: transparent; color: #667eea; border: 2px solid #667eea; padding: 13px 28px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block; font-size: 16px;'>
                                             🛍️ Tiếp tục mua sắm
                                         </a>
                                     </div>
                                     
                                     <p style='color: #666666; font-size: 16px; line-height: 1.6; margin-bottom: 15px;'>
                                         Cảm ơn bạn đã tin tưởng và lựa chọn BookShop. Chúc bạn có những trải nghiệm đọc sách thú vị!
                                     </p>
                                     
                                     <p style='color: #666666; font-size: 16px; line-height: 1.6; margin-bottom: 0;'>
                                         Trân trọng,<br>
                                         <strong style='color: #667eea;'>Đội ngũ BookShop</strong>
                                     </p>
                                 </td>
                             </tr>
                             
                             <!-- Footer -->
                             <tr>
                                 <td style='background-color: #f8f9ff; padding: 30px; text-align: center; border-radius: 0 0 10px 10px; border-top: 1px solid #e1e5e9;'>
                                     <p style='color: #999999; font-size: 14px; margin: 0 0 15px 0;'>
                                         📧 Email: no-reply@bookshop.com | 📞 Hotline: 0935-234-074
                                     </p>
                                     <p style='color: #999999; font-size: 12px; margin: 0;'>
                                         © 2025 BookShop. Tất cả quyền được bảo lưu.<br>
                                         Nếu bạn không muốn nhận email này, vui lòng <a href='#' style='color: #667eea;'>bỏ đăng ký</a>
                                     </p>
                                 </td>
                             </tr>
                         </table>
                     </body>
                     </html>
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
    
    private static string Vnd(decimal amount) => amount.ToString("C0", CultureInfo.GetCultureInfo("vi-VN"));
    
    private static OrderDetailRes Map(Order o)
    {
        var items = o.OrderItems.Select(i => new OrderItemDto(
            i.BookId,
            i.Book.CoverImage[0],
            i.Book?.Title,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice
            )).ToList();
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