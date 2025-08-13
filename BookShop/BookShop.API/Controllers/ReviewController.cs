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
public class ReviewController(IReviewService svc) : Controller
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewRes), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateReviewReq req)
    {
        var result = await svc.CreateAsync(User.GetUserId(), req);
        return CreatedAtAction(
            actionName: nameof(GetByBook),
            routeValues: new { bookId = result.BookId },
            value: GlobalResponse<ReviewRes>.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewRes), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateReviewReq req)
    {
        await svc.UpdateAsync(id, User.GetUserId(), req);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await svc.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("book/{bookId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewRes>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBook(
        [FromRoute] Guid bookId,
        [FromQuery] bool onlyVerified = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var reviews = await svc.GetByBookAsync(bookId, onlyVerified, page, pageSize);
        return Ok(GlobalResponse<IReadOnlyList<ReviewRes>>.Success(reviews));
    }

    [HttpGet("book/{bookId:guid}/avg")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAverage([FromRoute] Guid bookId)
    {
        var avg = await svc.GetAverageRatingAsync(bookId);
        return Ok(GlobalResponse<double>.Success(avg));
    }

    [HttpPost("{id:guid}/helpful")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Helpful([FromRoute] Guid id)
    {
        await svc.IncrementHelpfulAsync(id);
        return NoContent();
    }
}