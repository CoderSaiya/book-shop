namespace BookShop.Application.DTOs.Res;

public record AuthRes(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    UserRes User
    );