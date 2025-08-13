namespace BookShop.Application.Abstractions.Observability;

public interface IBookMetrics
{
    void Searched(bool hasKeyword, int pageSize);
    void Viewed(string source); 
    void Mutated(string action, string outcome);
}