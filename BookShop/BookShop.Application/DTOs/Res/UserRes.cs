namespace BookShop.Application.DTOs.Res;

public record UserRes(
    Guid UserId,
    string Email,
    string Name,
    string Phone,
    string Address,
    string? Avatar,
    DateTime CreatedAt
    );