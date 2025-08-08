using BookShop.Application.DTOs.Res;

namespace BookShop.Application.DTOs.Req;

public record UpdateAuthorReq(
    Guid AuthorId,
    string? Name,
    string? Bio
    );