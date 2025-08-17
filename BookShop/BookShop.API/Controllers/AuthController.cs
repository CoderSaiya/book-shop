using System.Security.Claims;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IHostEnvironment env
) : Controller
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterReq req)
    {
        await authService.RegisterAsync(req);
        return Created($"/api/auth/users/{Uri.EscapeDataString(req.Email)}",
            GlobalResponse<string>.Success("Đăng ký thành công"));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromForm] LoginReq req)
    {
        var res = await authService.LoginAsync(req);

        var isDev = env.IsDevelopment();
        Response.Cookies.Append("rt", res.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev
                ? SameSiteMode.Lax
                : SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(GlobalResponse<AuthRes>.Success(res));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var res = await authService.RefreshTokenAsync(refreshToken);
        if (res is null) return BadRequest(GlobalResponse<string>.Error("Refresh token expired or invalid."));

        return Ok(GlobalResponse<object>.Success(new { AccessToken = res }));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var isDev = env.IsDevelopment();
        Response.Cookies.Delete("rt", new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev
                ? SameSiteMode.Lax
                : SameSiteMode.None,
            Path = "/"
        });

        return Ok(GlobalResponse<string>.Success("Đã đăng xuất"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized(GlobalResponse<string>.Error("Missing sub/NameIdentifier.", StatusCodes.Status401Unauthorized));

        var userId = Guid.Parse(userIdStr);
        var dto = await authService.GetCurrentUserAsync(userId);
        return Ok(GlobalResponse<UserRes>.Success(dto));
    }
}