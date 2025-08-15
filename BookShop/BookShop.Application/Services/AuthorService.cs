using BookShop.Application.DTOs;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;

namespace BookShop.Application.Services;

public class AuthorService(
    IUnitOfWork uow,
    IEntityLocalizer lz
    ) : IAuthorService
{
    private const string SourceLang = "vi";
    private const string TargetLang = "en";
    
    public async Task<IEnumerable<AuthorRes>> GetAll()
    {
        var authors = await uow.Authors.ListAsync();

        var results = new List<AuthorRes>();
        foreach (var a in authors)
            results.Add(await MapAsync(a));

        return results;
    }
    
    public async Task<AuthorRes> GetById(Guid id)
    {
        ValidationHelper.Validate((id == Guid.Empty, "Id tác giả không được để trống"));

        var a = await uow.Authors.GetByIdAsync(id)
                ?? throw new NotFoundException("Author", id.ToString());

        return await MapAsync(a);
    }

    public async Task Create(CreateAuthorReq req)
    {
        ValidationHelper.Validate(
            (string.IsNullOrWhiteSpace(req.Name), "Tên tác giả không được để trống")
        );

        var author = new Author
        {
            Name = req.Name,
            Bio = req.Bio
        };
        
        await uow.Authors.AddAsync(author);
        await uow.SaveAsync();
    }

    public async Task Update(Guid authorId, UpdateAuthorReq req)
    {
        ValidationHelper.Validate(
            (authorId == Guid.Empty, "Id của tác giả không được để trống.")
        );
        
        var author = await uow.Authors.GetByIdAsync(authorId)
            ?? throw new NotFoundException("Author", authorId.ToString());
        
        if (req.Name is not null && req.Name != author.Name)
            author.Name = req.Name;
        
        if (req.Bio is not null)
            author.Bio = req.Bio;
        
        await uow.Authors.UpdateAsync(author);
        await uow.SaveAsync();
    }

    public async Task Delete(Guid id)
    {
        ValidationHelper.Validate(
            (id == Guid.Empty, "Id của tác giả không được để trống."),
            (!await uow.Authors.ExistsAsync(id), "Tác giả không tồn tại.")
        );
        
        await uow.Authors.DeleteAsync(id);
        await uow.SaveAsync();
    }
    
    private async Task<AuthorRes> MapAsync(Author a)
    {
        var bio = await LocalizeOptionalAsync(
            entityType: "Author",
            entityKey : a.Id.ToString(),
            field     : "Bio",
            viValue   : a.Bio
        );

        var books = new List<BookRes>(a.Books.Count);
        foreach (var b in a.Books)
            books.Add(await MapBookAsync(b, a.Name));

        return new AuthorRes(
            AuthorId: a.Id,
            AuthorName: a.Name,
            Bio: bio,
            Books: books
        );
    }

    private async Task<BookRes> MapBookAsync(Book b, string authorNameFallback)
    {
        var title = await LocalizeRequiredAsync("Book", b.Id.ToString(), "Title", b.Title);
        var desc  = await LocalizeOptionalAsync("Book", b.Id.ToString(), "Description", b.Description);
        var cat   = await LocalizeRequiredAsync("Category", b.CategoryId.ToString(), "Name", b.Category.Name);

        return new BookRes(
            BookId: b.Id,
            AuthorName: authorNameFallback,
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
            Category: new CategoryDto(
                b.CategoryId,
                cat)
        );
    }
    
    private async Task<LocalizedTextDto> LocalizeRequiredAsync(string entityType, string entityKey, string field, string viValue)
    {
        var en = await lz.LocalizeFieldAsync(entityType, entityKey, field, viValue, SourceLang, TargetLang);
        return new LocalizedTextDto(Vi: viValue, En: en);
    }

    private async Task<LocalizedTextDto?> LocalizeOptionalAsync(string entityType, string entityKey, string field, string? viValue)
    {
        if (string.IsNullOrWhiteSpace(viValue))
            return new LocalizedTextDto();

        var en = await lz.LocalizeFieldAsync(entityType, entityKey, field, viValue, SourceLang, TargetLang);
        return new LocalizedTextDto(Vi: viValue, En: en);
    }
}