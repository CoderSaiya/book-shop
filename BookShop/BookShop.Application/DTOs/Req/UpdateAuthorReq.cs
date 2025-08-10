using BookShop.Application.DTOs.Res;

namespace BookShop.Application.DTOs.Req;

public record UpdateAuthorReq(
    string? Name = null,
    string? Bio = null
    );