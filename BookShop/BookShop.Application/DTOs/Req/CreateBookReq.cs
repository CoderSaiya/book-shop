using Microsoft.AspNetCore.Http;

namespace BookShop.Application.DTOs.Req;

public record CreateBookReq(
    Guid AuthorId,
    Guid PublisherId,
    string Title,
    List<IFormFile> Images,
    decimal? Price = 0m,
    int? Stock = 0,
    string? Description = null,
    DateTime PublishingDate = default
    );