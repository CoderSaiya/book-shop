using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IAuthorRepository
{
    Task<Author?> GetWithBookAsync(Guid authorId);
}