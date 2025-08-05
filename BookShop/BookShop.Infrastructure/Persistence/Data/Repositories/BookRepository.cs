using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class BookRepository(AppDbContext context) : GenericRepository<Book>(context), IBookRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<Book>> SearchAsync(string keyword, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Array.Empty<Book>();
        
        var searchTerm = $"\"{keyword}\"";

        var hits = await _context.BookSearches
            .Where(bs => EF.Functions.Contains(
                new[] { bs.Title, bs.AuthorName, bs.PublisherName },
                searchTerm
            ))
            .Take(limit)
            .Select(bs => bs.BookId)
            .ToListAsync();
        
        if (!hits.Any())
            return Array.Empty<Book>();
        
        var books = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Where(b => hits.Contains(b.Id))
            .ToListAsync();
        
        return books;
    }

    public async Task<IEnumerable<Book>> GetByAuthorAsync(Guid authorId) =>
        await _context.Books
            .Where(b => b.AuthorId == authorId)
            .ToListAsync();

    public async Task<IEnumerable<Book>> GetByPublisher(Guid publisherId) =>
        await _context.Books
            .Where(b => b.PublisherId == publisherId)
            .ToListAsync();
}