using BookShop.Application.DTOs;
using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;
using BookShop.Domain.ValueObjects;

namespace BookShop.Application.Services;

public class PublisherService(IUnitOfWork uow) : IPublisherService
{
    public async Task<IEnumerable<PublisherRes>> GetAll() =>
        (await uow.Publishers.ListAsync()).Select(p => new PublisherRes(
            PublisherId: p.Id,
            PublisherName: p.Name,
            Address: new AddressDto(
                p.Address.Street,
                p.Address.Ward,
                p.Address.District,
                p.Address.CityOrProvince
                ),
            Website: p.Website,
            Books: p.Books.Select(b => new BookRes(
                BookId: b.Id,
                AuthorName: p.Name,
                PublisherName: b.Publisher.Name,
                Title: b.Title,
                Description: b.Description ?? string.Empty,
                Stock: b.Stock,
                Price: b.Price,
                Images: b.CoverImage.ToList(),
                PublishedDate: b.PublishedDate.ToString("dd/MM/yyyy"),
                IsSold: b.Stock <= 0
            ))
        ));

    public async Task<PublisherRes> GetById(Guid id)
    {
        ValidationHelper.Validate(
            (id == Guid.Empty, "Id nhà xuất bản không được để trống")
        );

        var p = await uow.Publishers.GetByIdAsync(id)
                ?? throw new NotFoundException("Publisher", id.ToString());
        
        return new PublisherRes(
            PublisherId: p.Id,
            PublisherName: p.Name,
            Address: new AddressDto(
                p.Address.Street,
                p.Address.Ward,
                p.Address.District,
                p.Address.CityOrProvince
            ),
            Website: p.Website,
            Books: p.Books.Select(b => new BookRes(
                BookId: b.Id,
                AuthorName: p.Name,
                PublisherName: b.Publisher.Name,
                Title: b.Title,
                Description: b.Description ?? string.Empty,
                Stock: b.Stock,
                Price: b.Price,
                Images: b.CoverImage.ToList(),
                PublishedDate: b.PublishedDate.ToString("dd/MM/yyyy"),
                IsSold: b.Stock <= 0
            ))
        );
    }

    public async Task Create(CreatePublisherReq req)
    {
        ValidationHelper.Validate(
            (string.IsNullOrWhiteSpace(req.Name), "Tên nhà xuất bản không được để trống")
        );

        var publisher = new Publisher
        {
            Name = req.Name,
            Address = Address.Create(
                req.Address.Street,
                req.Address.Ward,
                req.Address.District,
                req.Address.CityOrProvince
                ),
            Website = req.Website,
        };
        
        await uow.Publishers.AddAsync(publisher);
        await uow.SaveAsync();
    }

    public async Task Update(Guid publisherId, UpdatePublisherReq req)
    {
        ValidationHelper.Validate(
            (publisherId == Guid.Empty, "Id nhà xuất bản không được để trống")
        );
        
        var publisher = await uow.Publishers.GetByIdAsync(publisherId)
                     ?? throw new NotFoundException("Publisher", publisherId.ToString());
        
        if (req.Name is not null && req.Name != publisher.Name)
            publisher.Name = req.Name;
        
        if (req.Address is not null)
        {
            publisher.Address = Address.Create(
                req.Address.Street,
                req.Address.Ward,
                req.Address.District,
                req.Address.CityOrProvince);
        }
        
        if (req.Website is not null && req.Website != publisher.Website)
            publisher.Website = req.Website;
        
        await uow.Publishers.UpdateAsync(publisher);
        await uow.SaveAsync();
    }

    public async Task Delete(Guid id)
    {
        ValidationHelper.Validate(
            (id == Guid.Empty, "Id của nhà xuất bản không được để trống."),
            (!await uow.Publishers.ExistsAsync(id), "Nhà xuất bản không tồn tại.")
        );
        
        await uow.Publishers.DeleteAsync(id);
        await uow.SaveAsync();
    }
}