using BookShop.Application.DTOs.Req;
using BookShop.Application.Interface;
using BookShop.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(IPaymentFactory pf) : Controller
{
    [HttpPost("pay")]
    public async Task<IActionResult> Create([FromBody] PaymentReq req)
    {
        var resp = await pf.CreateAsync(req);
        return Ok(resp);
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(
        [FromQuery] PaymentProvider provider,
        [FromQuery] string orderId)
    {
        var resp = await pf.CheckAsync(provider, orderId);
        return Ok(resp);
    }
}