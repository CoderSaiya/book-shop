using BookShop.Application.DTOs;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using FluentValidation;

namespace BookShop.Application.Services;

public class CartService(IUnitOfWork uow) : ICartService
{
    public async Task<CartRes> GetActiveAsync(Guid userId)
    {
        var cart = await uow.Carts.EnsureActiveCartAsync(userId);
        return Map(cart);
    }

    public async Task<CartRes> AddOrUpdateItemAsync(Guid userId, AddCartItemReq req)
    {
        if (req.Quantity == 0) 
            throw new ValidationException("Số lượng phải khác 0.");
        
        var cart = await uow.Carts.EnsureActiveCartAsync(userId);
        
        await uow.Carts.AddOrUpdateItemAsync(cart.UserId, req.BookId, req.Quantity, req.UnitPrice);
        
        cart = await uow.Carts.GetActiveCartByUserAsync(userId, includeItems: true) 
               ?? throw new NotFoundException("Cart", cart.UserId.ToString());

        await uow.SaveAsync();
       
        return Map(cart);
    }

    public async Task RemoveItemAsync(Guid userId, Guid bookId)
    {
        var cart = await uow.Carts.GetActiveCartByUserAsync(userId, includeItems: false) 
                   ?? throw new NotFoundException("Cart của user", userId.ToString());
        
        await uow.Carts.RemoveItemAsync(cart.UserId, bookId);
        await uow.SaveAsync();
    }

    public async Task ClearAsync(Guid userId)
    {
        var cart = await uow.Carts.GetActiveCartByUserAsync(userId, includeItems: false) 
                   ?? throw new NotFoundException("Cart của user", userId.ToString());

        await uow.Carts.ClearAsync(cart.UserId);
        await uow.SaveAsync();
    }

    public async Task DeactivateAsync(Guid userId)
    {
        var cart = await uow.Carts.GetActiveCartByUserAsync(userId, includeItems: false);
        if (cart is not null) 
            await uow.Carts.DeactivateAsync(cart.UserId);
    }
    
    private static CartRes Map(Cart c)
    {
        var items = c.CartItems.Select(i => new CartItemDto(
            i.BookId,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice)
        ).ToList();
        
        return new CartRes(
            c.UserId,
            c.IsActive,
            c.TotalAmount,
            c.CreatedAt,
            c.UpdatedAt,
            items);
    }
}