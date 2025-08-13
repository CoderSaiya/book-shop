using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IAuthorRepository : IRepository<Author>
{
    Task<Author?> GetWithBookAsync(Guid authorId);
}