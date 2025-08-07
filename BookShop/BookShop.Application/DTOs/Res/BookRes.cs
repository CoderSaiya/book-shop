namespace BookShop.Application.DTOs.Res;

public record BookRes(
    Guid BookId,
    string AuthorName,
    string PublisherName,
    string Title,
    string Description,
    int Stock,
    List<string> Images,
    string PublishedDate,
    bool IsSold
    );