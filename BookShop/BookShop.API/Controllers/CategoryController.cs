using BookShop.Application.DTOs;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class CategoryController(ICategoryService svc)  : Controller
{
    [HttpPost]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryRes), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryReq req)
    {
        var result = await svc.CreateAsync(req);
        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = result.Id },
            value: GlobalResponse<CategoryRes>.Success(result));
    }

    [HttpPut("{id:guid}")]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryReq req)
    {
        await svc.UpdateAsync(id, req);
        return NoContent();
    }

    [HttpPut("{id:guid}/icon")]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateIcon(Guid id, [FromBody] UpdateCategoryIconReq req)
    {
        await svc.UpdateIconAsync(id, req.Icon);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await svc.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await svc.GetAsync(id);
        return Ok(GlobalResponse<CategoryRes>.Success(category));
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var categories = await svc.GetAllAsync();
        return Ok(GlobalResponse<IReadOnlyList<CategoryRes>>.Success(categories));
    }
}