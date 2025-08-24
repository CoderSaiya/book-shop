using BookShop.Application.Interface.AI;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(IIntentClassifier intent) : Controller
{
    [HttpGet("intent")]
    public IActionResult Classify([FromQuery] string text) {
        if (string.IsNullOrWhiteSpace(text)) return BadRequest("text required");
        var pred = intent.Predict(text);
        return Ok(new {
            label = pred.Label,
            confidence = pred.Confidence,
            scores = pred.Scores
        });
    }
}