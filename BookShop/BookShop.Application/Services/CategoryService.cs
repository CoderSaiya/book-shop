using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using FluentValidation;

namespace BookShop.Application.Services;

public class CategoryService(
    IUnitOfWork uow
    ) : ICategoryService
{
    public async Task<CategoryRes> CreateAsync(CreateCategoryReq req)
    {
        if (await uow.Categories.ExistsByNameAsync(req.Name))
            throw new ValidationException("Category đã tồn tại.");

        var c = new Category
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description,
            Icon = req.Icon
        };
        await uow.Categories.AddAsync(c);
        await uow.SaveAsync();
        
        return Map(c);
    }

    public async Task<CategoryRes> UpdateAsync(Guid id, UpdateCategoryReq req)
    {
        var c = await uow.Categories.GetByIdAsync(id) 
                ?? throw new NotFoundException("Category", id.ToString());
        
        if (!string.Equals(c.Name, req.Name, StringComparison.OrdinalIgnoreCase)
            && await uow.Categories.ExistsByNameAsync(req.Name))
            throw new ValidationException("Category đã tồn tại.");

        c.Name = req.Name;
        c.Description = req.Description;
        c.Icon = req.Icon;
        c.UpdatedAt = DateTime.UtcNow;

        await uow.Categories.UpdateAsync(c);
        await uow.SaveAsync();
        
        return Map(c);
    }

    public async Task UpdateIconAsync(Guid id, string? icon)
    {
        await uow.Categories.UpdateIconAsync(id, icon);
        await uow.SaveAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var c = await uow.Categories.GetByIdAsync(id) 
                ?? throw new NotFoundException("Category", id.ToString());
        
        await uow.Categories.DeleteAsync(c.Id);
        await uow.SaveAsync();
    }

    public async Task<CategoryRes> GetAsync(Guid id)
    {
        var c = await uow.Categories.GetByIdAsync(id) 
                ?? throw new NotFoundException("Category", id.ToString());
        
        return Map(c);
    }

    public async Task<IReadOnlyList<CategoryRes>> GetAllAsync()
    {
        var list = await uow.Categories.GetAllWithBookCountAsync();
        
        return list
            .Select(Map)
            .ToList();
    }
    
    private static CategoryRes Map(Category c)
        => new CategoryRes(
            c.Id,
            c.Name,
            c.Description,
            c.Icon,
            c.BookCount,
            c.CreatedAt);
}