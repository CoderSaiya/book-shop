namespace BookShop.Application.DTOs.Req;

public record UpdatePublisherReq(
    string? Name = null,
    AddressDto? Address = null,
    string? Website = null
    );