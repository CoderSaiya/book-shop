namespace BookShop.Application.DTOs;

public record CategoryDto (
    Guid Id,
    LocalizedTextDto Name
    );