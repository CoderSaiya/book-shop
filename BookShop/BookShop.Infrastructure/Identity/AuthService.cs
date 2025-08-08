using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using BookShop.Domain.Models;
using BookShop.Domain.ValueObjects;
using BookShop.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookShop.Infrastructure.Identity;

public class AuthService(
    IConfiguration configuration, 
    IUnitOfWork unitOfWork,
    IMailSender emailSender
    ) : IAuthService
{
    public async Task RegisterAsync(RegisterReq req)
    {
        if (await unitOfWork.Users.EmailExistsAsync(req.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var hashed = HashPassword(req.Password);
        var user = new User
        {
            Email = Email.Create(req.Email),
            Password = hashed,
        };
        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.SaveAsync();
        
        var emailMsg = new EmailMessage
        {
            ToEmail = req.Email,
            ToName  = req.Email,
            Subject = "🎉 Chào mừng đến với BookShop - Kho sách trực tuyến hàng đầu!",
            Body    = $@"
        <!DOCTYPE html>
        <html lang='vi'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Chào mừng đến với BookShop</title>
        </head>
        <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
            <table style='width: 100%; max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                <!-- Header -->
                <tr>
                    <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold;'>📚 BookShop</h1>
                        <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Kho sách trực tuyến hàng đầu</p>
                    </td>
                </tr>
                
                <!-- Main Content -->
                <tr>
                    <td style='padding: 40px 30px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h2 style='color: #333333; margin: 0; font-size: 24px;'>Chào mừng bạn đến với BookShop! 🎉</h2>
                        </div>
                        
                        <p style='color: #666666; font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                            Xin chào <strong style='color: #667eea;'>{req.Email}</strong>,
                        </p>
                        
                        <p style='color: #666666; font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                            Cảm ơn bạn đã đăng ký tài khoản tại <strong>BookShop</strong>! Chúng tôi rất vui mừng chào đón bạn trở thành thành viên của gia đình BookShop.
                        </p>
                        
                        <div style='background-color: #f8f9ff; border-left: 4px solid #667eea; padding: 20px; margin: 25px 0; border-radius: 5px;'>
                            <h3 style='color: #333333; margin: 0 0 15px 0; font-size: 18px;'>🎁 Ưu đãi đặc biệt dành cho bạn:</h3>
                            <ul style='color: #666666; margin: 0; padding-left: 20px;'>
                                <li style='margin-bottom: 8px;'>Giảm 15% cho đơn hàng đầu tiên</li>
                                <li style='margin-bottom: 8px;'>Miễn phí vận chuyển toàn quốc</li>
                                <li style='margin-bottom: 8px;'>Truy cập sớm vào các cuốn sách mới</li>
                            </ul>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='#' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block; font-size: 16px;'>
                                🛍️ Khám phá ngay
                            </a>
                        </div>
                        
                        <p style='color: #666666; font-size: 16px; line-height: 1.6; margin-bottom: 15px;'>
                            Chúc bạn có những trải nghiệm mua sắm tuyệt vời và tìm được những cuốn sách yêu thích!
                        </p>
                        
                        <p style='color: #666666; font-size: 16px; line-height: 1.6; margin-bottom: 0;'>
                            Trân trọng,<br>
                            <strong style='color: #667eea;'>Đội ngũ BookShop</strong>
                        </p>
                    </td>
                </tr>
                
                <!-- Footer -->
                <tr>
                    <td style='background-color: #f8f9ff; padding: 30px; text-align: center; border-radius: 0 0 10px 10px; border-top: 1px solid #e1e5e9;'>
                        <p style='color: #999999; font-size: 14px; margin: 0 0 15px 0;'>
                            📧 Email: support@bookshop.com | 📞 Hotline: 1900-BOOK-SHOP
                        </p>
                        <p style='color: #999999; font-size: 12px; margin: 0;'>
                            © 2025 BookShop. Tất cả quyền được bảo lưu.<br>
                            Nếu bạn không muốn nhận email này, vui lòng <a href='#' style='color: #667eea;'>bỏ đăng ký</a>
                        </p>
                    </td>
                </tr>
            </table>
        </body>
        </html>"
        };
        
        await emailSender.SendEmailAsync(emailMsg);
    }
    
    public async Task<AuthRes> LoginAsync(LoginReq req)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(req.Email)
                   ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
        if (!VerifyPassword(req.Password, user.Password))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email.ToString()),
            new Claim(ClaimTypes.Role, user.GetType().ToString())
        };
        var accessToken  = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();

        var rt = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };
        await unitOfWork.Refreshes.AddAsync(rt);
        await unitOfWork.SaveAsync();

        return new AuthRes(accessToken, refreshToken);
    }
    
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