using System.Net;
using System.Security.Claims;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using Microsoft.AspNetCore.Authentication;
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
        Response.Cookies.Append("refresh_token", res.RefreshToken, new CookieOptions
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
        Response.Cookies.Delete("refresh_token", new CookieOptions
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

    [HttpGet("external/{provider}/start")]
    [AllowAnonymous]
    public IActionResult External(
        string provider,
        [FromQuery] string? returnUrl,
        [FromServices] IConfiguration cfg)
    {
        var providers = new[] { "google", "github" };
        if (!providers.Contains(provider.ToLower()))
            return BadRequest("Unsupported provider");

        var fallbackReturn = $"{cfg["FrontendBaseUrl"]}/auth/sso/success";
        var encoded = WebUtility.UrlEncode(returnUrl ?? fallbackReturn);

        var redirectUri = $"/api/auth/external/callback?returnUrl={encoded}";
        var props = new AuthenticationProperties { RedirectUri = redirectUri };

        var scheme = provider.Equals("google", StringComparison.OrdinalIgnoreCase) ? "Google" : "GitHub";
        // Không cần mảng; 1 scheme là đủ
        return Challenge(props, scheme);
    }
    
    [HttpGet("external/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalCallback(
        [FromServices] IUserService users,
        [FromServices] IAuthService auth,
        [FromQuery] string? returnUrl,
        [FromServices] IConfiguration cfg)
    {
        // Dùng thuộc tính sẵn có thay vì tham số HttpContext
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded)
            return Redirect($"{cfg["FrontendBaseUrl"]}/auth/login?error=external_failed");

        var principal = result.Principal!;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var providerKey = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var provider = result.Ticket!.AuthenticationScheme; // "Google" | "GitHub"

        if (string.IsNullOrEmpty(providerKey))
            return Redirect($"{cfg["FrontendBaseUrl"]}/auth/login?error=missing_provider_key");

        var user = await users.FindOrCreateExternal(provider, providerKey, email, principal);
        var issued = await auth.IssueTokensForUserAsync(user);

        // refresh token -> cookie HttpOnly
        var isDev = env.IsDevelopment();
        Response.Cookies.Append("refresh_token", issued.RefreshToken, new CookieOptions {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        // access token trả về FE qua fragment
        var successUrl = string.IsNullOrEmpty(returnUrl)
            ? $"{cfg["FrontendBaseUrl"]}/auth/sso/success"
            : returnUrl!;
        var redirectWithToken =
            $"{successUrl}#access_token={WebUtility.UrlEncode(issued.AccessToken)}" +
            $"&expires_at={issued.AccessExpiresAt.ToUnixTimeSeconds()}&token_type=Bearer";

        await HttpContext.SignOutAsync("External");
        return Redirect(redirectWithToken);
    }
}