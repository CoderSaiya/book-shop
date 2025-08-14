namespace BookShop.Application.DTOs.Res;

public record AuthorRes(
    Guid AuthorId,
    string AuthorName,
    LocalizedTextDto? Bio,
    IEnumerable<BookRes> Books
    );