using BookShop.Domain.Common;
using BookShop.Domain.Entities;

namespace BookShop.Domain.Helpers;

public class PaymentMethodHelper
{
    private static readonly Dictionary<string, PaymentMethod> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cash_on_delivery"] = PaymentMethod.CashOnDelivery,
        ["cash-on-delivery"] = PaymentMethod.CashOnDelivery,
        ["cod"] = PaymentMethod.CashOnDelivery,
        ["cash"] = PaymentMethod.CashOnDelivery,

        ["credit_card"] = PaymentMethod.CreditCard,
        ["credit-card"] = PaymentMethod.CreditCard,
        ["credit"] = PaymentMethod.CreditCard,
        ["visa"] = PaymentMethod.CreditCard,
        ["mastercard"] = PaymentMethod.CreditCard,

        ["debit_card"] = PaymentMethod.DebitCard,
        ["debit-card"] = PaymentMethod.DebitCard,
        ["debit"] = PaymentMethod.DebitCard,

        ["bank_transfer"] = PaymentMethod.BankTransfer,
        ["bank-transfer"] = PaymentMethod.BankTransfer,
        ["wire"] = PaymentMethod.BankTransfer,
        ["bank"] = PaymentMethod.BankTransfer,

        ["paypal"] = PaymentMethod.PayPal,
        ["stripe"] = PaymentMethod.Stripe
    };
    
    public static PaymentMethod ParseOrThrow(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainValidationException("PaymentMethod không được rỗng.");

        var key = Normalize(input);

        if (Map.TryGetValue(key, out var m))
            return m;

        // fallback: cho phép client gửi đúng tên enum (CreditCard, creditcard, ...)
        if (Enum.TryParse<PaymentMethod>(input, ignoreCase: true, out var e))
            return e;

        var allowed = string.Join(", ", Enum.GetNames(typeof(PaymentMethod)));
        throw new DomainValidationException($"PaymentMethod không hợp lệ: '{input}'. Hợp lệ: {allowed}, hoặc alias phổ biến (cod, credit_card, bank_transfer, paypal, stripe...).");
    }

    private static string Normalize(string s)
        => s.Trim().Replace(' ', '_').Replace('-', '_').ToLowerInvariant();
}