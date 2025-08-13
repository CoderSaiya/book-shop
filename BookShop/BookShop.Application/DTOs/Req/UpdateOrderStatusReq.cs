using BookShop.Domain.Entities;

namespace BookShop.Application.DTOs.Req;

public record UpdateOrderStatusReq(OrderStatus OrderStatus);