namespace BookShop.Domain.Specifications;

public class VnPaySettings
{
    public string TmnCode { get; set; } = null!;
    public string HashSecret { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public string PaymentUrl { get; set; } = null!; // https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
    public string QueryUrl { get; set; } = null!; // https://sandbox.vnpayment.vn/merchant_webapi/api/transaction
    public string Version { get; set; } = "2.1.0";
    public string Locale { get; set; } = "vn";
    public string CurrCode { get; set; } = "VND";
    public int ExpireMinutes { get; set; } = 15;
}