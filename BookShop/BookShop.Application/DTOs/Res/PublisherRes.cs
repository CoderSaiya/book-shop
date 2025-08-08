namespace BookShop.Application.DTOs.Res;

public record PublisherRes(
    Guid PublisherId,
    string PublisherName,
    AddressDto Address,
    string Website,
    IEnumerable<BookRes> Books
    );