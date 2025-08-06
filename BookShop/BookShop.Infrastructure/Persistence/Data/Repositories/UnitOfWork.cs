using BookShop.Domain.Interfaces;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class UnitOfWork(
    AppDbContext context, 
    IAuthorRepository authors, 
    IBookRepository books, 
    IUserRepository users, 
    IRefreshTokenRepository refreshes, 
    IPublisherRepository publishers
    ) : IUnitOfWork
{
    public IAuthorRepository Authors { get; } = authors;
    public IBookRepository Books { get; } = books;
    public IUserRepository Users { get; } = users;
    public IRefreshTokenRepository Refreshes { get; } = refreshes;
    public IPublisherRepository Publishers { get; } = publishers;

    public async Task<int> SaveAsync()
        => await context.SaveChangesAsync();
}