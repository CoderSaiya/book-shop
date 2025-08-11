using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BookShop.Application.Services;

public class BookService(IUnitOfWork unitOfWork) : IBookService
{
    public async Task<IEnumerable<BookRes>> Search(string keyword = "", int page = 1, int pageSize = 50) => 
        (await unitOfWork.Books.SearchAsync(keyword, page, pageSize)).Select(b => new BookRes(
            BookId: b.Id,
            AuthorName: b.Author.Name,
            PublisherName: b.Publisher.Name,
            Title: b.Title,
            Description: b.Description ?? "",
            Stock: b.Stock,
            Images: b.CoverImage.ToList(),
            PublishedDate: b.PublishedDate.ToString("dd/MM/yyyy"),
            IsSold: b.Stock <= 0
            ));
    
    public async Task<BookRes> GetById(Guid bookId)
    {
        ValidationHelper.Validate(
            (bookId == Guid.Empty, "Id của sách không được để trống.")
        );

        var b = await unitOfWork.Books.GetByIdAsync(bookId)
                ?? throw new NotFoundException("Sách", bookId.ToString());

        return new BookRes(
            BookId: b.Id,
            AuthorName: b.Author.Name,
            PublisherName: b.Publisher.Name,
            Title: b.Title,
            Description: b.Description ?? string.Empty,
            Stock: b.Stock,
            Images: b.CoverImage.ToList(),
            PublishedDate: b.PublishedDate.ToString("dd/MM/yyyy"),
            IsSold: b.Stock <= 0
        );
    }

    public async Task Create(CreateBookReq request)
    {
        ValidationHelper.Validate(
            (request.AuthorId == Guid.Empty, "Id của tác giả không được để trống."),
            (!await unitOfWork.Authors.ExistsAsync(request.AuthorId), "Tác giả không tồn tại"),
            (request.PublisherId == Guid.Empty, "Id của nhà xuất bản không được để trống."),
            (!await unitOfWork.Publishers.ExistsAsync(request.AuthorId), "Nhà xuất bản không tồn tại"),
            (string.IsNullOrWhiteSpace(request.Title), "Tiêu đề không được để trống."),
            (!request.Images.Any(), "Phải có ít nhất một hình ảnh."),
            (request.Price < 0, "Giá phải là số không âm."),
            (request.Stock < 0, "Số lượng phải là số không âm.")
        );
        
        var book = new Book
        {
            AuthorId = request.AuthorId,
            PublisherId = request.PublisherId,
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
}