using Microsoft.AspNetCore.Http;

namespace BookShop.Application.DTOs.Req;

public record UpdateBookReq(
    Guid? AuthorId = null, 
    Guid? PublisherId = null, 
    string? Title = null, 
    string? Description = null, 
    decimal? Price = null, 
    int? Stock = null,
    DateTime? PublishedDate = null, 
    List<IFormFile>? Images = null);