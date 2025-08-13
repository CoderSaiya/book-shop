namespace BookShop.Application.DTOs.Req;

public record UpdateReviewReq(
    int Rating,
    string? Comment);