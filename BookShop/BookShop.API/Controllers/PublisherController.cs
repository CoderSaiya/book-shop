using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublisherController(IPublisherService publisherService) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(GlobalResponse<IEnumerable<PublisherRes>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlls()
    {
        var publisher = await publisherService.GetAll();
        return Ok(GlobalResponse<IEnumerable<PublisherRes>>.Success(publisher));
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GlobalResponse<PublisherRes>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var publisher = await publisherService.GetById(id);
        return Ok(GlobalResponse<PublisherRes>.Success(publisher));
    }
    
    [HttpPut("profile/{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProfile([FromRoute] Guid id, [FromForm] UpdatePublisherReq req)
    {
        await publisherService.Update(id, req);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await publisherService.Delete(id);
        return NoContent();
    }
}