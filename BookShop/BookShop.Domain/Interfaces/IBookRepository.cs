using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IBookRepository : IRepository<Book>
{
    Task<IEnumerable<Book>> SearchAsync(string keyword, int limit = 50, int page = 1);
    Task<IEnumerable<Book>> GetByAuthorAsync(Guid authorId);
    Task<IEnumerable<Book>> GetByPublisher(Guid publisherId);
}