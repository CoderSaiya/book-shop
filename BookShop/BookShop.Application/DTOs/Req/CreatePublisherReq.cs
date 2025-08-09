namespace BookShop.Application.DTOs.Req;

public record CreatePublisherReq(
    string Name,
    AddressDto Address,
    string Website
    );