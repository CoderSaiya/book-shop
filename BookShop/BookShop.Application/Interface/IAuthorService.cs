using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IAuthorService
{
    Task<IEnumerable<AuthorRes>> GetAll();
    Task<AuthorRes> GetById(Guid id);
    Task Create(CreateAuthorReq req);
    Task Update(Guid authorId, UpdateAuthorReq req);
    Task Delete(Guid id);
}