using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService svc) : Controller
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(OrderDetailRes), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOrderReq req)
    {
        var userId = User.GetUserId();
        var result = await svc.CreateAsync(userId, req);
        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new{ id = result.Id },
            value: GlobalResponse<OrderDetailRes>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(OrderDetailRes), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var order = await svc.GetDetailAsync(id);
        return Ok(GlobalResponse<OrderDetailRes>.Success(order));
    }
        

    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummaryRes>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<OrderSummaryRes>> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => svc.GetByUserAsync(User.GetUserId(), page, pageSize);

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateOrderStatusReq req)
    {
        await svc.UpdateStatusAsync(id, req.OrderStatus);
        return NoContent();
    }

    [HttpPatch("{id:guid}/payment")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePayment(
        [FromRoute] Guid id,
        [FromBody] UpdatePaymentReq req)
    {
        await svc.UpdatePaymentAsync(id, req.PaymentStatus, req.PaidAt);
        return NoContent();
    }

    [HttpGet("revenue")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<IActionResult> Revenue(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var revenue = await svc.GetRevenueAsync(fromUtc, toUtc);
        return Ok(GlobalResponse<decimal>.Success(revenue));
    }
}