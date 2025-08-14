namespace BookShop.Application.DTOs.Res;

public record CategoryRes(
    Guid Id,
    LocalizedTextDto Name,
    LocalizedTextDto? Description,
    string? Icon,
    int BookCount,
    DateTime CreatedAt);