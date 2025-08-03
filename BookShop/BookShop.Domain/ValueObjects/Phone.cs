using System.Text.RegularExpressions;

namespace BookShop.Domain.ValueObjects;

public record Phone
{
    private static readonly Regex DigitsOnly = new("^\\d+$", RegexOptions.Compiled);
    public string? CountryCode { get; init; }
    public string? SubscriberNumber { get; init; }
    
    private Phone(string countryCode, string subscriberNumber)
    {
        CountryCode = countryCode;
        SubscriberNumber = subscriberNumber;
    }
    
    public static Phone Create(string countryCode, string subscriberNumber)
    {
        ValidatePart(countryCode, 1, 3, nameof(countryCode));
        ValidatePart(subscriberNumber, 4, 15, nameof(subscriberNumber));

        return new Phone(countryCode, subscriberNumber);
    }
    
    public static Phone Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Phone number cannot be empty.", nameof(input));
        
        var digits = Regex.Replace(input, "[^\\d]", "");
        
        if (digits.Length < 5)
            throw new ArgumentException("Phone number is too short.", nameof(input));
        
        var countryCode = digits.Substring(0, 1);
        var subscriber = digits.Substring(1);

        return Create(countryCode, subscriber);
    }
    
    private static void ValidatePart(string value, int minLen, int maxLen, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} cannot be empty.", name);

        if (!DigitsOnly.IsMatch(value))
            throw new ArgumentException($"{name} must contain digits only.", name);

        if (value.Length < minLen || value.Length > maxLen)
            throw new ArgumentException(
                $"{name} must be between {minLen} and {maxLen} digits.", name);
    }
    
    public override string ToString() => $"+{CountryCode} {SubscriberNumber}";
}