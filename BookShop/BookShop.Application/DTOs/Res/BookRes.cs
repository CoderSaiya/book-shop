namespace BookShop.Application.DTOs.Res;

public record BookRes(
    Guid BookId,
    string AuthorName,
    string PublisherName,
    LocalizedTextDto Title,
    LocalizedTextDto? Description,
    int Stock,
    decimal Price,
    List<string> Images,
    string PublishedDate,
    bool IsSold,
    CategoryDto Category
    );