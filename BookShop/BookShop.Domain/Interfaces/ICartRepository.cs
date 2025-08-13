using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetActiveCartByUserAsync(Guid userId, bool includeItems = true);
    Task<Cart> EnsureActiveCartAsync(Guid userId);
    
    Task AddOrUpdateItemAsync(Guid cartId, Guid bookId, int quantity, decimal unitPrice);
    Task RemoveItemAsync(Guid cartId, Guid bookId);
    Task ClearAsync(Guid cartId);
    Task DeactivateAsync(Guid cartId);
}