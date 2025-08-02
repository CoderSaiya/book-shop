using System.Text.RegularExpressions;

namespace BookShop.Domain.ValueObjects;

public record Address
{
    private static readonly Regex ValidPart = new(
        @"^[\p{L}0-9\s\-./]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Street { get; init; } = null!;
    public string Ward { get; init; } = null!;
    public string District { get; init; } = null!;
    public string CityOrProvince { get; init; } = null!;
    
    private Address(
        string street,
        string ward,
        string district,
        string cityOrProvince)
    {
        Street = street;
        Ward  = ward;
        District = district;
        CityOrProvince = cityOrProvince;
    }
    
    public static Address Create(
        string street,
        string ward,
        string district,
        string cityOrProvince)
    {
        ValidatePart(street, 2, 100, nameof(street));
        ValidatePart(ward, 2, 100, nameof(ward));
        ValidatePart(district, 2, 100, nameof(district));
        ValidatePart(cityOrProvince, 2, 100, nameof(cityOrProvince));

        return new Address(
            street.Trim(),
            ward.Trim(),
            district.Trim(),
            cityOrProvince.Trim());
    }
    
    private static void ValidatePart(string value, int minLen, int maxLen, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} cannot be empty.", name);

        var trimmed = value.Trim();
        if (trimmed.Length < minLen || trimmed.Length > maxLen)
            throw new ArgumentException(
                $"{name} must be between {minLen} and {maxLen} characters.", name);

        if (!ValidPart.IsMatch(trimmed))
            throw new ArgumentException(
                $"{name} contains invalid characters.", name);
    }
    
    public override string ToString() => $"{Street}, {Ward}, {District}, {CityOrProvince}";
}