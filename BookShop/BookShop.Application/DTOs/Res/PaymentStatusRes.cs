namespace BookShop.Application.DTOs.Res;

public record PaymentStatusRes(
    string OrderId,
    string Status,
    string Message);