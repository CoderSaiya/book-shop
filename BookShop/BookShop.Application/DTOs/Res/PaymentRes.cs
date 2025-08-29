namespace BookShop.Application.DTOs.Res;

public record PaymentRes(
    string PayUrl, 
    string OrderId);