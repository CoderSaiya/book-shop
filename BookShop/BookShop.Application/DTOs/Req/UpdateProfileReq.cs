using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookShop.Application.DTOs.Req;

public record UpdateProfileReq(
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    [ValidateNever]
    AddressDto? Address = null,
    DateOnly? DateOfBirth = null,
    IFormFile? Avatar = null
    );