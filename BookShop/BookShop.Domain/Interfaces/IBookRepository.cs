using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IBookRepository
{
    Task<IEnumerable<Book>> SearchAsync(string keyword, int limit = 50);
    Task<IEnumerable<Book>> GetByAuthorAsync(Guid authorId);
    Task<IEnumerable<Book>> GetByPublisher(Guid publisherId);
}