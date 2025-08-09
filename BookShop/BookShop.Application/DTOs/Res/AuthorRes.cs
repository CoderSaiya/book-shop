namespace BookShop.Application.DTOs.Res;

public record AuthorRes(
    Guid AuthorId,
    string AuthorName,
    string? Bio,
    IEnumerable<BookRes> Books
    );