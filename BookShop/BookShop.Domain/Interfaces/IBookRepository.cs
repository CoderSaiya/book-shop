using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IBookRepository : IRepository<Book>
{
    Task<IEnumerable<Book>> SearchAsync(string keyword, int limit = 50, int page = 1);
    Task<IReadOnlyList<Book>> GetTrendingAsync(int days = 30, int limit = 12);
    Task<IEnumerable<Book>> GetByAuthorAsync(Guid authorId);
    Task<IEnumerable<Book>> GetByPublisher(Guid publisherId);
    Task<IReadOnlyList<Book>> GetRelatedAsync(Guid bookId, int days = 180, int limit = 12);
}