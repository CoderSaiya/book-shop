using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;

namespace BookShop.Application.Services;

public class AuthorService(IUnitOfWork uow) : IAuthorService
{
    public async Task<IEnumerable<AuthorRes>> GetAll() => 
        (await uow.Authors.ListAsync()).Select(a => new AuthorRes(
            AuthorId: a.Id,
            AuthorName: a.Name,
            Bio: a.Bio,
            Books: a.Books.Select(b => new BookRes(
                BookId: b.Id,
                AuthorName: a.Name,
                PublisherName: b.Publisher.Name,
                Title: b.Title,
                Description: b.Description ?? string.Empty,
                Stock: b.Stock,
                Images: b.CoverImage.ToList(),
                PublishedDate: b.PublishedDate.ToString("dd/MM/yyyy"),
                IsSold: b.Stock <= 0
                ))
            ));
    
    public async Task<AuthorRes> GetById(Guid id)
    {
        ValidationHelper.Validate(
            (id == Guid.Empty, "Id tác giả không được để trống")
        );

        var a = await uow.Authors.GetByIdAsync(id)
            ?? throw new NotFoundException("Author", id.ToString());

        return new AuthorRes(
            AuthorId: a.Id,
            AuthorName: a.Name,
            Bio: a.Bio,
            Books: a.Books.Select(b => new BookRes(
                BookId: b.Id,
                AuthorName: a.Name,
                PublisherName: b.Publisher.Name,
                Title: b.Title,
                Description: b.Description ?? string.Empty,
                Stock: b.Stock,
                Images: b.CoverImage.ToList(),
                PublishedDate: b.PublishedDate.ToString("dd/MM/yyyy"),
                IsSold: b.Stock <= 0
            ))
        );
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
}