using BookShop.Application.DTOs.Req;
using BookShop.Application.Interface;
using BookShop.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(
    IPaymentFactory pf,
    IOrderService ord
    ) : Controller
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
    
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentReq req)
    {
        var status = await pf.CheckAsync(req.Provider, req.PaymentOrderId);
        var isPaid = string.Equals(status.Status, "success", StringComparison.OrdinalIgnoreCase);

        if (!isPaid)
            return Ok(new { isPaid = false, status = status.Status, message = status.Message });
        
        await ord.MarkPaidAsync(req.ShopOrderId, req.Provider);
        
        return Ok(new { isPaid = true, orderId = req.ShopOrderId });
    }
}