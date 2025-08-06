using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookShop.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookShop.Infrastructure.Identity;

public class AuthService(
    IConfiguration configuration, 
    IUnitOfWork unitOfWork
    )
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new Byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<string?> RefreshTokenAsync(string refreshToken)
    {
        var existingToken = await unitOfWork.Refreshes.GetByTokenAsync(refreshToken);
        if (existingToken is null || existingToken.ExpiresAt < DateTime.Now || existingToken.IsRevoked)
            return null;

        existingToken.ExpiresAt = existingToken.ExpiresAt.AddMinutes(-1);
        
        var user = existingToken.User;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email.Address),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.GetType().ToString())
        };
        
        var newAccessToken = GenerateAccessToken(claims);
        
        return newAccessToken;
    }
}