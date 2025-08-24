using BookShop.Domain.ValueObjects;

namespace BookShop.Application.DTOs.Res;

public record ChatBotRes(
    string Text,
    string Intent,
    float Confidence,
    IEnumerable<object>? Books = null,
    IEnumerable<BotAction>? Actions = null);