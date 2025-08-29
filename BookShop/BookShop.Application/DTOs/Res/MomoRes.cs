namespace BookShop.Application.DTOs.Res;

public record MomoRes(
    int ResultCode,
    string Message,
    string? OrderId,
    string RequestId,
    string PayUrl,
    string Deeplink,
    string QrCodeUrl,
    string? Status,
    string? LocalMessage
);