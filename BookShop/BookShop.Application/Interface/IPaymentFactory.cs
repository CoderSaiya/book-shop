using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Domain.ValueObjects;

namespace BookShop.Application.Interface;

public interface IPaymentFactory
{
    Task<PaymentRes> CreateAsync(PaymentReq request);
    Task<PaymentStatusRes> CheckAsync(PaymentProvider provider, string orderId);
}