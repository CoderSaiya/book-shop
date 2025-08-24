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

    public override Task<Book?> GetByIdAsync(Guid id)
    {
        return _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IReadOnlyList<Book>> GetTrendingAsync(int days = 30, int limit = 12)
    {
        var since = DateTime.UtcNow.AddDays(-Math.Max(1, days));
        
        // Bán trong window days với đơn đã Paid
        var soldQuery =
            from oi in _context.OrderItems
            join o in _context.Orders on oi.OrderId equals o.Id
            where o.PaymentStatus == PaymentStatus.Paid && o.CreatedAt >= since
            group oi by oi.BookId into g
            select new
            {
                BookId = g.Key,
                SoldQty = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.TotalPrice)
            };

        // Review
        var reviewQuery =
            from r in _context.Reviews
            group r by r.BookId into g
            select new
            {
                BookId = g.Key,
                AvgRating = g.Average(x => (double)x.Rating),
                ReviewCount = g.Count()
            };

        // Join + tính điểm
        var q =
            from b in _context.Books
            
            join s in soldQuery on b.Id equals s.BookId into sj
            from s in sj.DefaultIfEmpty()
            join rv in reviewQuery on b.Id equals rv.BookId into rj
            from rv in rj.DefaultIfEmpty()
            let soldQty = (int?)s.SoldQty ?? 0
            let avgRating = (double?)rv.AvgRating ?? 0.0
            let reviewCnt = (int?)rv.ReviewCount ?? 0
            
            let score = (soldQty * 3.0) + (avgRating * 2.0) + (reviewCnt > 0 ? Math.Log10(reviewCnt + 1.0) : 0.0)
            orderby score descending, b.PublishedDate descending
            
            select b;

        return await q
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Category)
            .AsNoTracking()
            .Take(limit)
            .ToListAsync();
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

    public async Task<IReadOnlyList<Book>> GetRelatedAsync(Guid bookId, int days = 180, int limit = 12)
    {
        if (bookId == Guid.Empty) return Array.Empty<Book>();
        
        var current = await _context.Books
            .AsNoTracking()
            .Where(b => b.Id == bookId)
            .Select(b => new { b.Id, b.AuthorId, b.CategoryId })
            .FirstOrDefaultAsync();

        if (current is null) return Array.Empty<Book>();

        var since = DateTime.UtcNow.AddDays(-Math.Max(1, days));

        // Các sách được mua chung (co-purchase) với bookId trong các đơn Paid gần đây
        var coPurchaseQuery =
            from oi in _context.OrderItems
            join o in _context.Orders on oi.OrderId equals o.Id
            where o.PaymentStatus == PaymentStatus.Paid && o.CreatedAt >= since
            join oi2 in _context.OrderItems on oi.OrderId equals oi2.OrderId
            where oi.BookId == bookId && oi2.BookId != bookId
            group oi2 by oi2.BookId into g
            select new
            {
                BookId = g.Key,
                CoPurchaseQty = g.Sum(x => x.Quantity)
            };

        // Thống kê review để tăng/giảm điểm
        var reviewStatsQuery =
            from r in _context.Reviews
            group r by r.BookId into g
            select new
            {
                BookId = g.Key,
                AvgRating = g.Average(x => (double)x.Rating),
                ReviewCount = g.Count()
            };

        // Hợp nhất, tính điểm và sắp xếp
        var q =
            from b in _context.Books
            where b.Id != bookId
            join cp in coPurchaseQuery on b.Id equals cp.BookId into cpj
            from cp in cpj.DefaultIfEmpty()
            join rv in reviewStatsQuery on b.Id equals rv.BookId into rvj
            from rv in rvj.DefaultIfEmpty()
            let sameAuthor = b.AuthorId == current.AuthorId ? 1 : 0
            let sameCategory = b.CategoryId == current.CategoryId ? 1 : 0
            let co = (int?)cp.CoPurchaseQty ?? 0
            let rating = (double?)rv.AvgRating ?? 0.0
            let rcount = (int?)rv.ReviewCount ?? 0
            // Trọng số: co-purchase (5), cùng tác giả (3), cùng thể loại (1.5), rating (0.5), log(reviewCount)
            let score = (co * 5.0)
                        + (sameAuthor * 3.0)
                        + (sameCategory * 1.5)
                        + (rating * 0.5)
                        + (rcount > 0 ? Math.Log10(rcount + 1.0) : 0.0)
            orderby score descending, sameAuthor descending, b.PublishedDate descending
            select b;

        return await q
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Category)
            .AsNoTracking()
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetByCategoryAsync(List<Guid> ids) =>
        await context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Include(b => b.Publisher)
            .Include(b => b.Reviews)
            .AsNoTracking()
            .Where(b => ids.Contains(b.CategoryId))
            .ToListAsync();
}