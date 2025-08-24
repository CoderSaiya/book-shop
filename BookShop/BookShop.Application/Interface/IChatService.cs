using BookShop.Application.DTOs.Res;

namespace BookShop.Application.Interface;

public interface IChatService
{
    Task<ChatBotRes> ProcessAsync(Guid sessionId, string userMessage);
}