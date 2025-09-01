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
public class CouponController(ICouponService svc) : Controller
{
    [HttpPost("grant")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GlobalResponse<CouponRes>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Grant([FromForm] CreateCouponReq req)
    {
        var c = await svc.GrantAsync(req);
        return Created($"/api/user-coupons/{c.Id}", GlobalResponse<CouponRes>.Success(c));
    }

    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(GlobalResponse<IEnumerable<CouponRes>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mine([FromQuery] bool includeUsed = true)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await svc.ListMineAsync(userId, includeUsed);
        return Ok(GlobalResponse<IEnumerable<CouponRes>>.Success(list));
    }

    [HttpPost("preview")]
    [Authorize]
    [ProducesResponseType(typeof(GlobalResponse<ValidateCouponRes>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate([FromBody] ValidateCouponReq req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var r = await svc.ValidateAsync(userId, req);
        return Ok(GlobalResponse<ValidateCouponRes>.Success(r));
    }

    [HttpPost("use")]
    [Authorize]
    [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Use([FromBody] ValidateCouponReq req, [FromQuery] string? context = null)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await svc.UseAsync(userId, req.Code, context);
        return Ok(GlobalResponse<string>.Success("Đánh dấu đã dùng."));
    }
    
    [HttpGet("eligible")]
    [Authorize]
    [ProducesResponseType(typeof(GlobalResponse<IEnumerable<EligibleCouponRes>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Eligible([FromQuery] decimal subtotal)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await svc.ListEligibleAsync(userId, subtotal);
        return Ok(GlobalResponse<IEnumerable<EligibleCouponRes>>.Success(list));
    }
}