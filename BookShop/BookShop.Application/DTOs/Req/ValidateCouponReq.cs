namespace BookShop.Application.DTOs.Req;

public record ValidateCouponReq(string Code, decimal Subtotal);