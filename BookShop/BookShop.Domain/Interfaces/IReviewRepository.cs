using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<bool> ExistsByUserAndBookAsync(Guid userId, Guid bookId);
    Task<IReadOnlyList<Review>> GetByBookAsync(Guid bookId, bool onlyVerified = false, int page = 1, int pageSize = 20);
    Task<double> GetAverageRatingAsync(Guid bookId);
    Task IncrementHelpfulAsync(Guid reviewId);
    Task<Review?> GetUserReviewAsync(Guid userId, Guid bookId);
}