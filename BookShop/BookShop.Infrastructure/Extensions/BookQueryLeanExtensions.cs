using BookShop.Domain.Entities;

namespace BookShop.Infrastructure.Extensions;

public static class BookQueryLeanExtensions
{
    public static IQueryable<Book> AsListLean(this IQueryable<Book> q) =>
        q.Select(b => new Book {
            Id = b.Id,
            AuthorId = b.AuthorId,
            PublisherId = b.PublisherId,
            CategoryId = b.CategoryId,
            Title = b.Title,
            Description = b.Description,
            Stock = b.Stock,
            Price = b.Price,
            Sale = b.Sale,
            PublishedDate = b.PublishedDate,
            Author   = b.Author,
            Publisher= b.Publisher,
            Category = b.Category,
            CoverImage  = Array.Empty<string>(),
            CoverThumbs = string.IsNullOrEmpty(b.PrimaryThumb) ? Array.Empty<string>() : new[] { b.PrimaryThumb },
            PrimaryThumb = b.PrimaryThumb
        });
    
    public static IQueryable<Category> AsListLean(this IQueryable<Category> q) =>
        q.Select(c => new Category {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Icon = c.Icon,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            Books = c.Books.Select(b => new Book {
                Id = b.Id,
                AuthorId = b.AuthorId,
                PublisherId = b.PublisherId,
                CategoryId = b.CategoryId,
                Title = b.Title,
                Description = b.Description,
                Stock = b.Stock,
                Price = b.Price,
                Sale = b.Sale,
                PublishedDate = b.PublishedDate,
                Author   = b.Author,
                Publisher= b.Publisher,
                Category = b.Category,
                CoverImage  = Array.Empty<string>(),
                CoverThumbs = string.IsNullOrEmpty(b.PrimaryThumb) ? Array.Empty<string>() : new[] { b.PrimaryThumb },
                PrimaryThumb = b.PrimaryThumb
            }).ToList()
        });
}