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
public class UserController(IUserService userService) : Controller
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GlobalResponse<IEnumerable<UserRes>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string keyword = "",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var users = await userService.Search(keyword, page, pageSize);
        return Ok(GlobalResponse<IEnumerable<UserRes>>.Success(users));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(GlobalResponse<UserRes>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var users = await userService.GetById(id);
        return Ok(GlobalResponse<UserRes>.Success(users));
    }

    [HttpPut("profile/{id:guid}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProfile([FromRoute] Guid id, [FromForm] UpdateProfileReq req)
    {
        var me = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && !string.Equals(me, id.ToString(), StringComparison.OrdinalIgnoreCase))
            return Forbid();

        await userService.UpdateProfile(id, req);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var me = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && !string.Equals(me, id.ToString(), StringComparison.OrdinalIgnoreCase))
            return Forbid();
        
        await userService.Delete(id);
        return NoContent();
    }
    
}