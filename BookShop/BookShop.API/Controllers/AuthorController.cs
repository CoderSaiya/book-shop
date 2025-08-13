using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorController(IAuthorService authorService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GlobalResponse<IEnumerable<UserRes>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlls()
    {
        var authors = await authorService.GetAll();
        return Ok(GlobalResponse<IEnumerable<AuthorRes>>.Success(authors));
    }
    
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GlobalResponse<AuthorRes>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var author = await authorService.GetById(id);
        return Ok(GlobalResponse<AuthorRes>.Success(author));
    }
    
    [HttpPut("profile/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProfile([FromRoute] Guid id, [FromForm] UpdateAuthorReq req)
    {
        await authorService.Update(id, req);
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
        await authorService.Delete(id);
        return NoContent();
    }
}