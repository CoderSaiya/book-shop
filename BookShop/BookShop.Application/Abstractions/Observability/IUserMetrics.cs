namespace BookShop.Application.Abstractions.Observability;

public interface IUserMetrics
{
    void Viewed(string source);
    void Mutated(string action, string outcome);
}