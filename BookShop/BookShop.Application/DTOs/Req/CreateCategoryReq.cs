namespace BookShop.Application.DTOs.Req;

public record CreateCategoryReq(
    string Name,
    string? Description,
    string? Icon);