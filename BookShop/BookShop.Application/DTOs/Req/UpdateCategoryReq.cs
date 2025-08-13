namespace BookShop.Application.DTOs.Req;

public record UpdateCategoryReq(
    string Name,
    string? Description,
    string? Icon);