using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface ICategoryService
{
    Task<CategoryRes> CreateAsync(CreateCategoryReq req);
    Task<CategoryRes> UpdateAsync(Guid id, UpdateCategoryReq req);
    Task UpdateIconAsync(Guid id, string? icon);
    Task DeleteAsync(Guid id);
    Task<CategoryRes> GetAsync(Guid id);
    Task<IReadOnlyList<CategoryRes>> GetAllAsync();
}