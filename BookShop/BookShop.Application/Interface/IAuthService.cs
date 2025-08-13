using System.Security.Claims;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IAuthService
{
    Task RegisterAsync(RegisterReq req);
    Task<AuthRes> LoginAsync(LoginReq req);
    public string HashPassword(string password);
    public bool VerifyPassword(string password, string hashedPassword);
    public string GenerateAccessToken(IEnumerable<Claim> claims);
    public string GenerateRefreshToken();
    public Task<string?> RefreshTokenAsync(string refreshToken);
}