using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IPaymentGateway
{
    Task<PaymentRes> CreatePaymentIntentAsync(PaymentReq request);
    Task<PaymentStatusRes> CheckPaymentStatusAsync(string orderId);
}