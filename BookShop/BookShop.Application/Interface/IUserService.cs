using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IUserService
{
    Task<IEnumerable<UserRes>> Search(string keyword, int page = 1, int pageSize = 50);
    Task UpdateProfile(UpdateProfileReq req);
    Task Delete(Guid id);
}