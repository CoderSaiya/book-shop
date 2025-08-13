using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IReviewService
{
    Task<ReviewRes> CreateAsync(Guid userId, CreateReviewReq req);
    Task<ReviewRes> UpdateAsync(Guid reviewId, Guid userId, UpdateReviewReq req);
    Task DeleteAsync(Guid reviewId, Guid userId);
    Task<IReadOnlyList<ReviewRes>> GetByBookAsync(Guid bookId, bool onlyVerified, int page, int pageSize);
    Task<double> GetAverageRatingAsync(Guid bookId);
    Task IncrementHelpfulAsync(Guid reviewId);
}