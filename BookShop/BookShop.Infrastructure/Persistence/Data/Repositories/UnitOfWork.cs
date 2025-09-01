using BookShop.Domain.Interfaces;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class UnitOfWork(
    AppDbContext context, 
    IAuthorRepository authors, 
    IBookRepository books, 
    IUserRepository users, 
    IRefreshTokenRepository refreshes, 
    IPublisherRepository publishers,
    ICategoryRepository categories,
    IOrderRepository orders,
    ICartRepository carts,
    IReviewRepository reviews,
    ICouponRepository coupons
    ) : IUnitOfWork
{
    public IAuthorRepository Authors { get; } = authors;
    public IBookRepository Books { get; } = books;
    public IUserRepository Users { get; } = users;
    public IRefreshTokenRepository Refreshes { get; } = refreshes;
    public IPublisherRepository Publishers { get; } = publishers;
    public ICategoryRepository Categories { get; } = categories;
    public IOrderRepository Orders { get; } = orders;
    public ICartRepository Carts { get; } = carts;
    public IReviewRepository Reviews { get; } = reviews;
    public ICouponRepository Coupons { get; } = coupons;

    public async Task<int> SaveAsync()
        => await context.SaveChangesAsync();
}