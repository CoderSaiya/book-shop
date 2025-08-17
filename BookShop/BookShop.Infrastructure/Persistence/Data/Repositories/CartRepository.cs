using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class CartRepository(AppDbContext context) : GenericRepository<Cart>(context), ICartRepository
{
    private readonly AppDbContext _context = context;
    
    public async Task<Cart?> GetActiveCartByUserAsync(Guid userId, bool includeItems = true)
    {
        IQueryable<Cart> q = _context.Carts
            .Where(c => c.UserId == userId && c.IsActive);
        if (includeItems) q = q
            .Include(c => c.CartItems);
        
        return await q
            .FirstOrDefaultAsync();
    }

    public async Task<Cart> EnsureActiveCartAsync(Guid userId)
    {
        var cart = await GetActiveCartByUserAsync(userId, includeItems: true);
        if (cart is not null) return cart;

        cart = new Cart
        {
            UserId = userId,
            IsActive = true
        };
        
        await _context.Carts.AddAsync(cart);
        return cart;
    }

    public async Task AddOrUpdateItemAsync(Guid cartId, Guid bookId, int quantity, decimal unitPrice)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == cartId && c.IsActive);
        if (cart is null) return;

        var item = cart.CartItems.FirstOrDefault(i => i.BookId == bookId);
        if (item is null)
        {
            item = new CartItem
            {
                CartId = cart.UserId,
                BookId = bookId,
                Quantity = Math.Max(1, quantity),
                UnitPrice = unitPrice,
            };
            cart.CartItems.Add(item);
        }
        else
        {
            item.Quantity = Math.Max(1, item.Quantity + quantity);
            item.UnitPrice = unitPrice;
        }

        cart.UpdatedAt = DateTime.UtcNow;
    }

    public async Task RemoveItemAsync(Guid cartId, Guid bookId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.BookId == bookId);
        if (item is null) return;

        _context.CartItems.Remove(item);
    }

    public async Task ClearAsync(Guid cartId)
    {
        await _context.CartItems
            .Where(i => i.CartId == cartId)
            .ExecuteDeleteAsync();

        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == cartId);
        if (cart is not null)
        {
            cart.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task DeactivateAsync(Guid cartId)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == cartId);
        if (cart is null) return;

        cart.IsActive = false;
        cart.UpdatedAt = DateTime.UtcNow;
    }
}