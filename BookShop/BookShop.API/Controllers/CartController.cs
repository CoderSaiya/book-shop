using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController(ICartService svc) : Controller
{
    [HttpGet("active")]
    [ProducesResponseType(typeof(CartRes), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var cart = await svc.GetActiveAsync(User.GetUserId());
        return Ok(GlobalResponse<CartRes>.Success(cart));
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(CartRes), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddOrUpdate([FromBody] AddCartItemReq req)
    {
        var cart = await svc.AddOrUpdateItemAsync(User.GetUserId(), req);
        return Ok(GlobalResponse<CartRes>.Success(cart));
    }

    [HttpDelete("items/{bookId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(Guid bookId)
    {
        await svc.RemoveItemAsync(User.GetUserId(), bookId);
        return NoContent();
    }

    [HttpPost("clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Clear()
    {
        await svc.ClearAsync(User.GetUserId());
        return NoContent();
    }

    [HttpPost("deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate()
    {
        await svc.DeactivateAsync(User.GetUserId());
        return NoContent();
    }
}