using BookShop.Application.DTOs;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;
using FluentValidation;

namespace BookShop.Application.Services;

public class CategoryService(
    IUnitOfWork uow,
    IEntityLocalizer lz
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
        
        return await MapAsync(c);
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
        
        return await MapAsync(c);
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
        
        return await MapAsync(c);
    }

    public async Task<IReadOnlyList<CategoryRes>> GetAllAsync()
    {
        var list = await uow.Categories.GetAllWithBookCountAsync();
        
        var results = await Task.WhenAll(list.Select(MapAsync));
        return results.ToList();
    }

    public async Task<IReadOnlyList<CategoryMap>> MapNamesToIdsAsync(IEnumerable<string> names)
    {
        var list = names?.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).Distinct().ToList() ?? [];
        if (list.Count == 0) return [];

        // Lấy toàn bộ category 1 lần
        var cats = (await uow.Categories.ListAsync())
            .Select(c => new { c.Id, c.Name })
            .ToList();

        // So khớp không dấu, contains
        var result = new List<CategoryMap>();
        foreach (var q in list)
        {
            var qNorm = IntentHelper.RemoveDiacritics(q).ToLowerInvariant();

            var hit = cats.FirstOrDefault(c =>
                IntentHelper.RemoveDiacritics(c.Name).ToLowerInvariant().Contains(qNorm));

            if (hit is not null)
                result.Add(new CategoryMap(hit.Id, hit.Name));
        }

        return result;
    }

    private async Task<CategoryRes> MapAsync(Category c)
    {
        // Khóa cache: EntityType="Category", EntityKey: ưu tiên Slug nếu có, tạm dùng Id
        var enName = await lz.LocalizeFieldAsync(
            entityType: "Category",
            entityKey: c.Id.ToString(),
            field: "Name",
            sourceText: c.Name, 
            sourceLang: "vi",
            targetLang: "en");

        string? enDesc = null;
        if (!string.IsNullOrWhiteSpace(c.Description))
        {
            enDesc = await lz.LocalizeFieldAsync(
                entityType: "Category",
                entityKey: c.Id.ToString(),
                field: "Description",
                sourceText: c.Description,
                sourceLang: "vi",
                targetLang: "en");
        }
        
        var nameDto = new LocalizedTextDto(
            Vi: c.Name,
            En: enName
        );
        var descDto = new LocalizedTextDto(
            Vi: c.Description ?? "",
            En: enDesc ?? "");

        return new CategoryRes(
            c.Id,
            nameDto,
            descDto,
            c.Icon,
            c.BookCount,
            c.CreatedAt
        );
    }
}