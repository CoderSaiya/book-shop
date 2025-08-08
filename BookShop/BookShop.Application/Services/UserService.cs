using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Helpers;
using BookShop.Domain.Interfaces;
using BookShop.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace BookShop.Application.Services;

public class UserService(IUnitOfWork uow) : IUserService
{
    public async Task<IEnumerable<UserRes>> Search(string keyword, int page = 1, int pageSize = 50) =>
        (await uow.Users.SearchAsync(keyword, page, pageSize)).Select(u => new UserRes(
            UserId: u.Id,
            Email: u.Email.ToString(),
            Name: u.Profile.Name?.ToString() ?? "Unknown",
            Phone: u.Profile.Phone?.ToString() ?? "Unknown",
            Address: u.Profile.Address?.ToString() ?? "Unknown",
            Avatar: u.Profile.Avatar?.ToString() ?? null,
            CreatedAt: u.CreatedAt
        ));

    public async Task UpdateProfile(UpdateProfileReq req)
    {
        ValidationHelper.Validate(
            (req.UserId == Guid.Empty, "Id của người dùng không được để trống."),
            (!await uow.Users.ExistsAsync(req.UserId), "Người dùng không tồn tại.")
        );
        
        var user = await uow.Users.GetByIdWithProfileAsync(req.UserId)
                   ?? throw new InvalidOperationException("Người dùng không tồn tại.");

        var profile = user.Profile;
        
        var firstName = !string.IsNullOrWhiteSpace(req.FirstName)
            ? req.FirstName!
            : profile.Name?.FirstName ?? string.Empty;
        var lastName = !string.IsNullOrWhiteSpace(req.LastName)
            ? req.LastName!
            : profile.Name?.LastName ?? string.Empty;
        profile.Name = Name.Create(firstName, lastName);
        
        if (!string.IsNullOrWhiteSpace(req.PhoneNumber))
        {
            profile.Phone = Phone.Parse(req.PhoneNumber!);
        }

        if (req.Address is not null)
        {
            profile.Address = Address.Create(
                req.Address.Street,
                req.Address.Ward,
                req.Address.District,
                req.Address.CityOrProvince);
        }
        
        if (req.DateOfBirth.HasValue)
        {
            profile.DateOfBirth = req.DateOfBirth.Value;
        }
        
        if (req.Avatar is not null && req.Avatar.Length > 0)
        {
            var bytes = await ToByteArrayAsync(req.Avatar);
            profile.Avatar = Convert.ToBase64String(bytes);
        }
        
        user.Profile = profile;
        await uow.SaveAsync();
    }

    public async Task Delete(Guid id)
    {
        ValidationHelper.Validate(
            (id == Guid.Empty, "Id của người dùng không được để trống."),
            (!await uow.Users.ExistsAsync(id), "Người dùng không tồn tại.")
        );
        
        await uow.Users.DeleteAsync(id);
        await uow.SaveAsync();
    }
    
    private async Task<byte[]> ToByteArrayAsync(IFormFile file)
    {
        if (file.Length == 0)
            return [];

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}