using System.Text.RegularExpressions;

namespace BookShop.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Address { get; init; } = null!;
    
    private Email(string address)
    {
        Address = address;
    }
    
    public static Email Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Email cannot be empty.", nameof(address));

        if (!EmailRegex.IsMatch(address))
            throw new ArgumentException("Email is not in a valid format.", nameof(address));
        
        var normalized = address.Trim();
        return new Email(normalized);
    }
    
    public override string ToString() => Address;
}