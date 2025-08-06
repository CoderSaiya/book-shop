namespace BookShop.Domain.Interfaces;

public interface IUnitOfWork
{
    IAuthorRepository Authors { get; }
    IBookRepository Books { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository Refreshes { get; }
    IPublisherRepository Publishers { get; }
    
    Task<int> SaveAsync();
}