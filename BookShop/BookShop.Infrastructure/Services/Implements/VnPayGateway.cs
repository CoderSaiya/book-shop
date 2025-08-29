using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;
using BookShop.Domain.Specifications;
using Microsoft.Extensions.Options;

namespace BookShop.Infrastructure.Services.Implements;

public sealed class VnPayGateway(
    IOptions<VnPaySettings> vnPayOptions,
    HttpClient http,
    IUnitOfWork uow
) : IPaymentGateway
{
    private readonly VnPaySettings _opt = vnPayOptions.Value;

    public async Task<PaymentRes> CreatePaymentIntentAsync(PaymentReq request)
    {
        var orderId = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Random.Shared.Next(1000, 9999);
        var createDate = DateTime.UtcNow;
        var expireDate = createDate.AddMinutes(_opt.ExpireMinutes);

        var amountVnd = ((long)Math.Round(request.Amount)) * 100; // VNPay x100

        var data = new Dictionary<string, string>
        {
            ["vnp_Version"] = _opt.Version,
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _opt.TmnCode,
            ["vnp_Amount"] = amountVnd.ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = _opt.CurrCode,
            ["vnp_TxnRef"] = orderId,
            ["vnp_OrderInfo"] = request.OrderInfo ?? "Thanh toan don hang",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = _opt.ReturnUrl,
            ["vnp_IpAddr"] = request.ClientIp ?? "127.0.0.1",
            ["vnp_Locale"] = _opt.Locale,
            ["vnp_CreateDate"] = createDate.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = expireDate.ToString("yyyyMMddHHmmss")
        };

        var queryToSign = VnPayHelper.BuildQuery(data);
        var secureHash = VnPayHelper.HmacSHA512(_opt.HashSecret, queryToSign);

        var payUrl = $"{_opt.PaymentUrl}?{queryToSign}&vnp_SecureHash={secureHash}&vnp_SecureHashType=HmacSHA512";

        // Bạn nên lưu (orderId, createDate) để dùng cho API query trạng thái
        // ví dụ OrderRepository.Save(orderId, createDateUtc)

        return new PaymentRes(
            PayUrl: payUrl,
            OrderId: orderId
        );
    }

    public async Task<PaymentStatusRes> CheckPaymentStatusAsync(string orderId)
    {
        // VNPay queryDR cần vnp_TransactionDate (thời điểm tạo giao dịch, format yyyyMMddHHmmss)
        // => Lấy từ DB đã lưu khi tạo đơn
        var transactionDateUtc = await LoadTransactionDateUtc(orderId);
        if (transactionDateUtc == null)
        {
            return new PaymentStatusRes(orderId, "unknown", "Không có transactionDate để query VNPay (hãy lưu khi tạo).");
        }

        var requestId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        var data = new Dictionary<string, string>
        {
            ["vnp_Version"] = _opt.Version,
            ["vnp_Command"] = "querydr",
            ["vnp_TmnCode"] = _opt.TmnCode,
            ["vnp_TxnRef"] = orderId,
            ["vnp_OrderInfo"] = $"Query {orderId}",
            ["vnp_TransDate"] = transactionDateUtc.Value.ToString("yyyyMMddHHmmss"),
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
            ["vnp_IpAddr"] = "127.0.0.1"
        };

        var queryToSign = VnPayHelper.BuildQuery(data);
        var secureHash = VnPayHelper.HmacSHA512(_opt.HashSecret, queryToSign);
        data["vnp_SecureHash"] = secureHash;

        var resp = await http.PostAsJsonAsync(_opt.QueryUrl, data);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"VNPay status check failed {(int)resp.StatusCode}: {body}");

        // Phản hồi chuẩn có RspCode: "00" thành công, "01" pending, khác => lỗi
        using var doc = JsonDocument.Parse(body);
        var rspCode = doc.RootElement.TryGetProperty("RspCode", out var rc) ? rc.GetString() : null;
        var message = doc.RootElement.TryGetProperty("Message", out var msg) ? msg.GetString() : body;

        var status = rspCode switch
        {
            "00" => "success",
            "01" => "processing",
            _    => "failed"
        };

        return new PaymentStatusRes(orderId, status, message ?? "");
    }
    
    private async Task<DateTime?> LoadTransactionDateUtc(string orderId)
        => (await uow.Orders.GetByIdAsync(Guid.Parse(orderId)))?.CreatedAt;
}