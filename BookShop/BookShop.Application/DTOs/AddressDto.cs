namespace BookShop.Application.DTOs;

public record AddressDto(
    string Street,
    string Ward,
    string District,
    string CityOrProvince
    );