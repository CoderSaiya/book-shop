using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IPublisherService
{
    Task<IEnumerable<PublisherRes>> GetAll();
    Task<PublisherRes> GetById(Guid id);
    Task Create(CreatePublisherReq req);
    Task Update(Guid publisherId, UpdatePublisherReq req);
    Task Delete(Guid id);
}