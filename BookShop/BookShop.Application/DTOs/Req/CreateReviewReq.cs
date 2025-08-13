namespace BookShop.Application.DTOs.Req;

public record CreateReviewReq(
    Guid BookId,
    int Rating,
    string? Comment);