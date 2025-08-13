using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface ICartService
{
    Task<CartRes> GetActiveAsync(Guid userId);
    Task<CartRes> AddOrUpdateItemAsync(Guid userId, AddCartItemReq req);
    Task RemoveItemAsync(Guid userId, Guid bookId);
    Task ClearAsync(Guid userId);
    Task DeactivateAsync(Guid userId);
}