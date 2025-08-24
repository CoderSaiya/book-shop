using System.Collections.Concurrent;
using BookShop.Application.Interface;
using Microsoft.AspNetCore.SignalR;

namespace BookShop.Infrastructure.Hubs;

public class ChatHub(IChatService chat) : Hub
{
    public static ConcurrentDictionary<Guid, string> OnlineDoctors { get; } = new();
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null)
        {
            Console.WriteLine($"[ChatHub] OnConnectedAsync called. QueryString = {httpContext.Request.QueryString}");
            
            if (httpContext.Request.Query.TryGetValue("doctorId", out var docIdString) &&
                Guid.TryParse(docIdString, out var docId))
            {
                OnlineDoctors[docId] = Context.ConnectionId;
                await base.OnConnectedAsync();
                return;
            }
            
            var sessionIdStr = httpContext.Request.Query["sessionId"].ToString();
            if (Guid.TryParse(sessionIdStr, out var sessionId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetSessionGroupName(sessionId));
            }
        }

        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var httpContext = Context.GetHttpContext();
        Console.WriteLine($"[ChatHub] OnDisconnectedAsync called. Exception: {exception}");
        
        if (httpContext != null &&
            httpContext.Request.Query.TryGetValue("doctorId", out var docIdString) &&
            Guid.TryParse(docIdString, out var docId))
        {
            OnlineDoctors.TryRemove(docId, out _);
            await base.OnDisconnectedAsync(exception);
            return;
        }
        
        var sessionIdStr = httpContext.Request.Query["sessionId"].ToString();
        if (Guid.TryParse(sessionIdStr, out var sessionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetSessionGroupName(sessionId));
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    [HubMethodName("SendMessageAsync")]
     public async Task SendMessageAsync(Guid sessionId, string senderType, string content)
    {
        var groupName = GetSessionGroupName(sessionId);

        try
        {
            // 1) Phát lại tin của user (nếu cần giữ nguyên hành vi cũ)
            var payloadUser = new
            {
                id = Guid.NewGuid(),
                sessionId,
                senderType,
                content,
                createdAt = DateTime.UtcNow
            };
            await Clients.OthersInGroup(groupName).SendAsync("ReceiveMessage", payloadUser);

            // 2) Nếu là User -> xử lý bằng Orchestrator
            if (senderType.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                var bot = await chat.ProcessAsync(sessionId, content);

                // Gửi text của Bot
                var payloadAi = new
                {
                    id = Guid.NewGuid(),
                    sessionId,
                    senderType = "Bot",
                    content = bot.Text,
                    intent = bot.Intent,
                    confidence = bot.Confidence,
                    books = bot.Books,
                    createdAt = DateTime.UtcNow
                };
                await Clients.Group(groupName).SendAsync("ReceiveMessage", payloadAi);

                // Nếu có Action (ví dụ AddToCart), gửi kênh riêng cho FE xử lý
                if (bot.Actions is not null)
                {
                    await Clients.Group(groupName).SendAsync("ReceiveAction", bot.Actions);
                }
            }
        }
        catch (Exception ex)
        {
            var errorPayload = new
            {
                id = Guid.NewGuid(),
                sessionId,
                senderType = "Bot",
                content = "Đã xảy ra lỗi nội bộ khi xử lý yêu cầu. Vui lòng thử lại sau.",
                createdAt = DateTime.UtcNow
            };
            await Clients.Group(groupName).SendAsync("ReceiveMessage", errorPayload);
            Console.Error.WriteLine($"[ChatHub] Exception: {ex}");
        }
    }
    
    public static string GetSessionGroupName(Guid sessionId) => $"chat-session-{sessionId}";
}