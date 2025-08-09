using Microsoft.AspNetCore.Http;

namespace BookShop.Application.DTOs.Req;

public record UpdateProfileReq(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    AddressDto? Address,
    DateOnly? DateOfBirth,
    IFormFile? Avatar
    );