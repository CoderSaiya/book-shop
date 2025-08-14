using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController(IBookService bookService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GlobalResponse<IEnumerable<BookRes>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string keyword = "",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var books = await bookService.Search(keyword, page, pageSize);
        return Ok(GlobalResponse<IEnumerable<BookRes>>.Success(books));
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GlobalResponse<IEnumerable<BookRes>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrending([FromQuery] int days = 30, [FromQuery] int limit = 12)
    {
        var books = await bookService.GetTrendingAsync(days, limit);
        return Ok(GlobalResponse<IEnumerable<BookRes>>.Success(books));
    }
    
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GlobalResponse<BookRes>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var book = await bookService.GetById(id);
        return Ok(GlobalResponse<BookRes>.Success(book));
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromForm] CreateBookReq req)
    {
        await bookService.Create(req);
        return NoContent();
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateBookReq req)
    {
        await bookService.Update(id, req);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await bookService.Delete(id);
        return NoContent();
    }
}