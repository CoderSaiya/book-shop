using BookShop.Application.DTOs.Req;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : Controller
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
        
        Response.Cookies.Append("rt", res.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
        
        return Ok(GlobalResponse<object>.Success(new { res.AccessToken }));
    }
    
    [HttpPost("refresh-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var res = await authService.RefreshTokenAsync(refreshToken);
        if (res is null) return BadRequest(GlobalResponse<string>.Error("Refresh token expired or invalid."));
        
        return Ok(GlobalResponse<object>.Success(new { AccessToken = res }));
    }
}