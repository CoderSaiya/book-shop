namespace BookShop.Domain.Interfaces;

public interface IUnitOfWork
{
    IAuthorRepository Authors { get; }
    IBookRepository Books { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository Refreshes { get; }
    IPublisherRepository Publishers { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    ICartRepository Carts { get; }
    IReviewRepository Reviews { get; }
    
    Task<int> SaveAsync();
}