namespace BookShop.Domain.ValueObjects;

public sealed record Name
{
    public string? FirstName { get; init; } = null;
    public string? LastName { get; init; } = null;

    public Name(string? firstName, string? lastName)
    {
        if(!string.IsNullOrWhiteSpace(firstName)) FirstName = firstName;
        if(!string.IsNullOrWhiteSpace(lastName)) LastName = lastName;
    }

    public static Name Create(string firstName, string lastName)
    {
        return new Name(firstName, lastName);
    }

    public override string ToString() => $"{FirstName} {LastName}";
}