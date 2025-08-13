namespace BookShop.Application.DTOs.Res;

public record CategoryRes(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int BookCount,
    DateTime CreatedAt);