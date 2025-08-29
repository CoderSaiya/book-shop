using System.Security.Cryptography;
using System.Text;

namespace BookShop.Domain.Helpers;

public static class VnPayHelper
{
    public static string HmacSHA512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public static string BuildQuery(IDictionary<string, string> data)
    {
        // VNPay yêu cầu sort theo key tăng dần trước khi ký
        var sorted = data.OrderBy(kv => kv.Key, StringComparer.Ordinal);
        return string.Join("&", sorted.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
    }
}