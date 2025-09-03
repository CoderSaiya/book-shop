using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface ISessionMemory
{
    void SaveRecommendations(Guid sessionId, IReadOnlyList<BookRes> books);
    IReadOnlyList<BookRes> GetRecommendations(Guid sessionId);
}