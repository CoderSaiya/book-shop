using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.ValueObjects;

namespace BookShop.Infrastructure.Services.Implements;

public class PaymentFactory(
    MomoGateway momo,
    VnPayGateway vnpay
    ): IPaymentFactory
{
    public async Task<PaymentRes> CreateAsync(PaymentReq request) =>
        request.Provider switch
        {
            PaymentProvider.MoMo => await momo.CreatePaymentIntentAsync(request),
            PaymentProvider.VnPay => await vnpay.CreatePaymentIntentAsync(request),
            _ => throw new ArgumentOutOfRangeException()
        };

    public async Task<PaymentStatusRes> CheckAsync(PaymentProvider provider, string orderId) =>
        provider switch
        {
            PaymentProvider.MoMo => await momo.CheckPaymentStatusAsync(orderId),
            PaymentProvider.VnPay => await vnpay.CheckPaymentStatusAsync(orderId),
            _ => throw new ArgumentOutOfRangeException()
        };
}