using System.Security.Claims;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Domain.Entities;

namespace BookShop.Application.Interface;

public interface IAuthService
{
    Task<UserRes> GetCurrentUserAsync(Guid userId);
    Task RegisterAsync(RegisterReq req);
    Task<AuthRes> LoginAsync(LoginReq req);
    public string HashPassword(string password);
    public bool VerifyPassword(string password, string hashedPassword);
    public string GenerateAccessToken(IEnumerable<Claim> claims);
    public string GenerateRefreshToken();
    public Task<string?> RefreshTokenAsync(string refreshToken);
    Task<(string AccessToken, string RefreshToken, DateTimeOffset AccessExpiresAt)> IssueTokensForUserAsync(User user);
}