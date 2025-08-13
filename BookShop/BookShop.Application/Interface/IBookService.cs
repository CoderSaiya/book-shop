using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IBookService
{
    Task<IEnumerable<BookRes>> Search(string keyword = "", int page = 1, int pageSize = 50);
    Task<BookRes> GetById(Guid bookId);
    Task Create(CreateBookReq request);
    Task Update(Guid bookId, UpdateBookReq request);
    Task Delete(Guid bookId);
}