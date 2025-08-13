using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Common;
using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using FluentValidation;

namespace BookShop.Application.Services;

public class ReviewService(
    IUnitOfWork uow
    ) : IReviewService
{
    public async Task<ReviewRes> CreateAsync(Guid userId, CreateReviewReq req)
    {
        if (req.Rating < 1 || req.Rating > 5)
            throw new ValidationException("Rating phải từ 1 đến 5.");

        // Xác thực đã mua (đơn đã Paid và chứa book)
        var purchased = await PurchasedAsync(userId, req.BookId);

        if (await uow.Reviews.ExistsByUserAndBookAsync(userId, req.BookId))
            throw new ValidationException("Bạn đã đánh giá sách này.");

        var r = new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BookId = req.BookId,
            Rating = req.Rating,
            Comment = req.Comment,
            IsVerifiedPurchase = purchased
        };
        
        await uow.Reviews.AddAsync(r);
        await uow.SaveAsync();
        
        return Map(r);
    }

    public async Task<ReviewRes> UpdateAsync(Guid reviewId, Guid userId, UpdateReviewReq req)
    {
        var r = await uow.Reviews.GetByIdAsync(reviewId) 
                ?? throw new NotFoundException("Review", reviewId.ToString());
        if (r.UserId != userId) 
            throw new UnauthorizedAccessException();

        r.Rating = req.Rating is >= 1 and <= 5 ? req.Rating : throw new DomainValidationException("Rating phải từ 1 đến 5.");
        r.Comment = req.Comment;
        r.UpdatedAt = DateTime.UtcNow;

        await uow.Reviews.UpdateAsync(r);
        await uow.SaveAsync();
        
        return Map(r);
    }

    public async Task DeleteAsync(Guid reviewId, Guid userId)
    {
        var r = await uow.Reviews.GetByIdAsync(reviewId) 
                ?? throw new NotFoundException("Review", reviewId.ToString());
        
        if (r.UserId != userId) 
            throw new UnauthorizedAccessException();
        
        await uow.Reviews.DeleteAsync(r.Id);
        await uow.SaveAsync();
    }

    public async Task<IReadOnlyList<ReviewRes>> GetByBookAsync(Guid bookId, bool onlyVerified, int page, int pageSize)
    {
        var list = await uow.Reviews.GetByBookAsync(bookId, onlyVerified, page, pageSize);
        return list.Select(Map).ToList();
    }

    public Task<double> GetAverageRatingAsync(Guid bookId) =>
        uow.Reviews.GetAverageRatingAsync(bookId);

    public Task IncrementHelpfulAsync(Guid reviewId) =>
        uow.Reviews.IncrementHelpfulAsync(reviewId);
    
    private async Task<bool> PurchasedAsync(Guid userId, Guid bookId)
    {
        var orders = await uow.Orders.GetByUserAsync(userId, 1, 200);
        return orders.Any(o => o.PaymentStatus == PaymentStatus.Paid);
    }

    private static ReviewRes Map(Review r) =>
        new ReviewRes(
            r.Id,
            r.UserId,
            r.BookId,
            r.Rating,
            r.Comment,
            r.IsVerifiedPurchase,
            r.HelpfulCount,
            r.CreatedAt);
}