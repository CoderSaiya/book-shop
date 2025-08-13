using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class BookRepository(AppDbContext context) : GenericRepository<Book>(context), IBookRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<Book>> SearchAsync(string keyword, int limit = 50, int page = 1)
    {
        // if (IsEmptySearch(keyword))
        //     return Array.Empty<Book>();

        var words = keyword!
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(EscapeLike)
            .Select(w => w + "%")
            .ToList();
        
        var q = _context.BookSearches
            .FromSqlRaw(@"SELECT BookId, Title, AuthorName, PublisherName 
                      FROM dbo.vwBookSearch WITH (NOEXPAND)")
            .AsNoTracking();
        
        foreach (var p in words)
        {
            var pat = p;
            q = q.Where(bs =>
                EF.Functions.Like(bs.Title, pat) ||
                EF.Functions.Like(bs.AuthorName, pat) ||
                EF.Functions.Like(bs.PublisherName, pat));
        }

        var hits = await q
            .OrderBy(x => x.Title)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(x => x.BookId)
            .ToListAsync();

        if (hits.Count == 0) return Array.Empty<Book>();

        var books = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Category)
            .Include(b => b.Reviews)
            .Where(b => hits.Contains(b.Id))
            .ToListAsync();

        return books;
    }
    
    private static string EscapeLike(string s) =>
        s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
    
    private static bool IsEmptySearch(string? k)
    {
        if (string.IsNullOrWhiteSpace(k)) return true;

        var s = k.Trim();
        
        if ((s.StartsWith('"') && s.EndsWith('"')) ||
            (s.StartsWith('\'') && s.EndsWith('\'')))
        {
            s = s.Substring(1, s.Length - 2).Trim();
        }
        
        s = s.Trim('*').Trim();

        return string.IsNullOrWhiteSpace(s);
    }

    private static string BuildContainsTerm(string k)
    {
        var s = k.Trim();
        
        if ((s.StartsWith('"') && s.EndsWith('"')) ||
            (s.StartsWith('\'') && s.EndsWith('\'')))
        {
            return s;
        }
        
        return $"\"{s}\"";
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