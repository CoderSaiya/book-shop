using System.Collections.Concurrent;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;

namespace BookShop.Application.Services;

public sealed class InMemorySessionMemory : ISessionMemory
{
    private readonly ConcurrentDictionary<Guid, List<BookRes>> _recos = new();

    public void SaveRecommendations(Guid sessionId, IReadOnlyList<BookRes> books)
        => _recos[sessionId] = books.ToList();

    public IReadOnlyList<BookRes> GetRecommendations(Guid sessionId)
        => _recos.TryGetValue(sessionId, out var list) ? list : [];
}