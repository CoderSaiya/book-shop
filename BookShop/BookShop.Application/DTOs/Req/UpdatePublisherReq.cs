namespace BookShop.Application.DTOs.Req;

public record UpdatePublisherReq(
    Guid PublisherId,
    string? Name = null,
    AddressDto? Address = null,
    string? Website = null
    );