using BookShop.Application.DTOs;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BookShop.Application.Services;

public class BookService(
    IUnitOfWork unitOfWork,
    IEntityLocalizer lz
    ) : IBookService
{
    private const string SourceLang = "vi";
    private const string TargetLang = "en";
    
    public async Task<IEnumerable<BookRes>> Search(string keyword = "", int page = 1, int pageSize = 50)
    {
        var books = await unitOfWork.Books.SearchAsync(keyword, pageSize, page);

        var results = new List<BookRes>();
        foreach (var b in books)
            results.Add(await MapAsync(b));

        return results;
    }

    public async Task<IReadOnlyList<BookRes>> GetTrendingAsync(int days = 30, int limit = 12)
    {
        var books = await unitOfWork.Books.GetTrendingAsync(days, limit);

        var list = new List<BookRes>(books.Count);
        foreach (var b in books)
            list.Add(await MapAsync(b));

        return list;
    }

    public async Task<IReadOnlyList<BookRes>> RecommendByCategories(IEnumerable<Guid> categoryIds, int limit, decimal? minPrice, decimal? maxPrice, string? keyword)
    {
        var ids = categoryIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0) return [];

        // base query
        var q = await unitOfWork.Books.GetByCategoryAsync(ids);

        if (minPrice is not null) q = q.Where(b => b.CurrentPrice >= minPrice);
        if (maxPrice is not null) q = q.Where(b => b.CurrentPrice <= maxPrice);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            q = q.Where(b => b.Title.Contains(kw) || (b.Description != null && b.Description.Contains(kw)));
        }
        
        q = q.OrderByDescending(b => b.OrderItems.Count)
            .ThenByDescending(b => b.Reviews.Count)
            .ThenByDescending(b => b.PublishedDate);

        var tasks = q.Select(MapAsync);
        var mapped = await Task.WhenAll(tasks);
        return mapped;
    }

    public async Task<BookRes> GetById(Guid bookId)
    {
        ValidationHelper.Validate((bookId == Guid.Empty, "Id của sách không được để trống."));

        var b = await unitOfWork.Books.GetByIdAsync(bookId)
                ?? throw new NotFoundException("Sách", bookId.ToString());

        return await MapAsync(b);
    }

    public async Task<IReadOnlyList<BookRes>> GetRelatedAsync(Guid bookId, int days = 180, int limit = 12)
    {
        ValidationHelper.Validate((bookId == Guid.Empty, "Id của sách không được để trống."));

        var books = await unitOfWork.Books.GetRelatedAsync(bookId, days, limit);

        var list = new List<BookRes>(books.Count);
        foreach (var b in books)
            list.Add(await MapAsync(b));

        return list;
    }

    public async Task Create(CreateBookReq request)
    {
        ValidationHelper.Validate(
            (request.AuthorId == Guid.Empty, "Id của tác giả không được để trống."),
            (!await unitOfWork.Authors.ExistsAsync(request.AuthorId), "Tác giả không tồn tại"),
            (request.PublisherId == Guid.Empty, "Id của nhà xuất bản không được để trống."),
            (!await unitOfWork.Publishers.ExistsAsync(request.PublisherId), "Nhà xuất bản không tồn tại"),
            (string.IsNullOrWhiteSpace(request.Title), "Tiêu đề không được để trống."),
            (!request.Images.Any(), "Phải có ít nhất một hình ảnh."),
            (request.Price < 0, "Giá phải là số không âm."),
            (request.Stock < 0, "Số lượng phải là số không âm.")
        );
        
        var book = new Book
        {
            AuthorId = request.AuthorId,
            PublisherId = request.PublisherId,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Price = request.Price!.Value,
            Stock = request.Stock!.Value,
            Description = request.Description?.Trim(),
            PublishedDate = request.PublishingDate
        };
        
        var base64Images = new List<string>();
        foreach (var image in request.Images)
        {
            var bytes = await ToByteArrayAsync(image);
            var base64 = $"data:{image.ContentType};base64,{Convert.ToBase64String(bytes)}";
            base64Images.Add(base64);
        }
        book.CoverImage = base64Images.ToArray();
        
        await unitOfWork.Books.AddAsync(book);
        await unitOfWork.SaveAsync();
    }

    public async Task Update(Guid bookId, UpdateBookReq request)
    {
        ValidationHelper.Validate(
            (bookId == Guid.Empty, "Id của sách không được để trống."),
            (request.AuthorId.HasValue && !await unitOfWork.Authors.ExistsAsync(request.AuthorId.Value),
                "Tác giả không tồn tại."),
            (request.PublisherId.HasValue && !await unitOfWork.Publishers.ExistsAsync(request.PublisherId.Value),
                "Nhà xuất bản không tồn tại."),
            (request.Price is < 0, "Giá phải là số không âm."),
            (request.Stock is < 0, "Số lượng phải là số không âm.")
            );
        
        var existingBook = await unitOfWork.Books.GetByIdAsync(bookId) 
                            ?? throw new NotFoundException("Sách", bookId.ToString());
        
        static void SetIf<T>(T? value, Action<T> setter) where T : struct
        {
            if (value.HasValue) setter(value.Value);
        }
        static void SetStr(string? s, Action<string> setter)
        {
            if (!string.IsNullOrWhiteSpace(s)) setter(s.Trim());
        }
        
        if (request.AuthorId.HasValue)
        {
            if (!await unitOfWork.Authors.ExistsAsync(request.AuthorId.Value))
                throw new Exception("Tác giả không tồn tại.");
            existingBook.AuthorId = request.AuthorId.Value;
        }
        if (request.PublisherId.HasValue)
        {
            if (!await unitOfWork.Publishers.ExistsAsync(request.PublisherId.Value))
                throw new Exception("Nhà xuất bản không tồn tại.");
            existingBook.PublisherId = request.PublisherId.Value;
        }

        SetStr(request.Title, v => existingBook.Title = v);
        SetStr(request.Description, v => existingBook.Description = v);
        SetIf(request.Price, v => existingBook.Price = v);
        SetIf(request.Stock, v => existingBook.Stock = v);
        SetIf(request.PublishedDate, v => existingBook.PublishedDate = v);

        if (request.Images is not null && request.Images.Any())
        {
            var base64Images = new List<string>();
            foreach (var image in request.Images)
            {
                var bytes = await ToByteArrayAsync(image);
                var base64 = $"data:{image.ContentType};base64,{Convert.ToBase64String(bytes)}";
                base64Images.Add(base64);
            }
            existingBook.CoverImage = base64Images.ToArray();
        }
        
        await unitOfWork.Books.UpdateAsync(existingBook);
        await unitOfWork.SaveAsync();
    }

    public async Task Delete(Guid bookId)
    {
        ValidationHelper.Validate(
            (bookId == Guid.Empty, "Id của sách không được để trống."),
            (!await unitOfWork.Books.ExistsAsync(bookId), "Sách không tồn tại.")
            );
        
        await unitOfWork.Books.DeleteAsync(bookId);
        await unitOfWork.SaveAsync();
    }
    
    private async Task<byte[]> ToByteArrayAsync(IFormFile file)
    {
        if (file.Length == 0)
            return [];

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
    
    private async Task<BookRes> MapAsync(Book b)
    {
        var title = await LocalizeRequiredAsync("Book", b.Id.ToString(), "Title", b.Title);
        var desc = await LocalizeOptionalAsync("Book", b.Id.ToString(), "Description", b.Description);
        var cat = await LocalizeRequiredAsync("Category", b.CategoryId.ToString(), "Name", b.Category.Name);

        return new BookRes(
            BookId: b.Id,
            AuthorName: b.Author.Name,
            PublisherName: b.Publisher.Name,
            Title: title,
            Description: desc,
            Stock: b.Stock,
            Price: b.Price,
            Sale: b.Sale,
            CurrentPrice: b.CurrentPrice,
            Images: b.CoverImage.ToList(),
            PublishedDate: b.PublishedDate.ToString("dd/MM/yyyy"),
            IsSold: b.Stock <= 0,
            Category: new CategoryDto(b.CategoryId, cat)
        );
    }

    private async Task<LocalizedTextDto> LocalizeRequiredAsync(string entityType, string entityKey, string field, string viValue)
    {
        var en = await lz.LocalizeFieldAsync(entityType, entityKey, field, viValue, SourceLang, TargetLang);
        return new LocalizedTextDto(Vi: viValue, En: en);
    }

    private async Task<LocalizedTextDto> LocalizeOptionalAsync(string entityType, string entityKey, string field, string? viValue)
    {
        if (string.IsNullOrWhiteSpace(viValue))
            return new LocalizedTextDto();

        var en = await lz.LocalizeFieldAsync(entityType, entityKey, field, viValue, SourceLang, TargetLang);
        return new LocalizedTextDto(Vi: viValue, En: en);
    }
}