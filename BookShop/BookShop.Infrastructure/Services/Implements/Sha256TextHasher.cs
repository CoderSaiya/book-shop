using System.Security.Cryptography;
using System.Text;
using BookShop.Application.Interface;

namespace BookShop.Infrastructure.Services.Implements;

public class Sha256TextHasher : ITextHasher
{
    public string ComputeHash(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}