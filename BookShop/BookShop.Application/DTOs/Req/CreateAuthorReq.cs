namespace BookShop.Application.DTOs.Req;

public record CreateAuthorReq(
    string Name,
    string? Bio = null
    );