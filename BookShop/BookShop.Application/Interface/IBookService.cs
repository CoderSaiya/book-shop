using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IBookService
{
    Task<IEnumerable<BookRes>> Search(string keyword = "", int page = 1, int pageSize = 50);
    Task<IReadOnlyList<BookRes>> GetTrendingAsync(int days = 30, int limit = 12);
    Task<IReadOnlyList<BookRes>> RecommendByCategories(
        IEnumerable<Guid> categoryIds, 
        int limit, 
        decimal? minPrice, 
        decimal? maxPrice, 
        string? keyword);
    Task<BookRes> GetById(Guid bookId);
    Task<IReadOnlyList<BookRes>> GetRelatedAsync(Guid bookId, int days = 180, int limit = 12);
    Task Create(CreateBookReq request);
    Task Update(Guid bookId, UpdateBookReq request);
    Task Delete(Guid bookId);
}