using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Application.Interface.AI;
using BookShop.Domain.Helpers;
using BookShop.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BookShop.Infrastructure.Services.Implements;

public class ChatService(
    IIntentClassifier classifier,
    IBookService bookService,
    ICategoryService categoryService,
    ILogger<ChatService> logger
) : IChatService
{
    // Áp ngưỡng tối thiểu để tin vào intent
    private const float MinIntentConfidence = 0.45f;

    public async Task<ChatBotRes> ProcessAsync(Guid sessionId, string userMessage)
    {
        var pred = classifier.Predict(userMessage);
        var intent = pred.Label;
        var conf = pred.Confidence;

        // nếu model đoán "goodbye/greeting" nhưng câu có từ khóa recommend
        if (conf < 0.55f && IntentHelper.LooksLikeRecommend(userMessage))
        {
            intent = "recommend";
            conf   = Math.Max(conf, 0.55f);
        }

        switch (intent)
        {
            case "recommend":
                return await HandleRecommend(userMessage, intent, conf);

            case "refine":
                return await HandleRefine(userMessage, intent, conf);

            case "add_to_cart":
                return await HandleAddToCart(userMessage, intent, conf);

            case "confirm_yes":
                return new ChatBotRes("Đã xác nhận 👍", intent, conf);

            case "confirm_no":
                return new ChatBotRes("Đã hủy thao tác ✋", intent, conf);

            case "greeting":
                return new ChatBotRes("Chào bạn 👋. Bạn muốn tìm sách thể loại nào và khoảng giá bao nhiêu?", intent, conf);

            case "goodbye":
                return new ChatBotRes("Cảm ơn bạn đã ghé shop. Hẹn gặp lại! 👋", intent, conf);

            default:
                // fallback
                return await HandleRecommend(userMessage, "recommend", conf * 0.8f);
        }
    }

    private async Task<ChatBotRes> HandleRecommend(string text, string intent, float conf)
    {
        var (min, max) = IntentHelper.ExtractPriceRange(text);
        
        var catNames = IntentHelper.ExtractCategoryNames(text);
        var catMaps  = (catNames.Count > 0) ? await categoryService.MapNamesToIdsAsync(catNames) : [];
        
        List<BookRes> top;

        if (catMaps.Count > 0)
        {
            // gọi service theo category
            top = (await bookService.RecommendByCategories(
                    categoryIds: catMaps.Select(c => c.Id),
                    limit: 8,
                    minPrice: min, maxPrice: max,
                    keyword: text))
                .ToList();
        }
        else
        {
            // fallback: logic cũ theo keyword
            var results = await bookService.Search(text, page: 1, pageSize: 40);
            var filtered = results;

            if (min is not null || max is not null)
            {
                filtered = filtered.Where(b =>
                {
                    var price = b.Price;
                    var okMin = min is null || price >= min.Value;
                    var okMax = max is null || price <= max.Value;
                    return okMin && okMax;
                }).ToList();
            }

            top = filtered.Take(8).ToList();
        }

        if (top.Count == 0)
        {
            var trend = await bookService.GetTrendingAsync(days: 30, limit: 8);
            top = trend.ToList();
        }

        // build câu trả lời
        string catText = (catMaps.Count > 0)
            ? $" theo thể loại {string.Join(", ", catMaps.Select(c => c.Name))}"
            : "";

        var textOut = (min, max) switch
        {
            (not null, not null) => $"Mình gợi ý vài cuốn{catText} trong khoảng {min:N0}–{max:N0}đ:",
            (not null, null) => $"Mình gợi ý vài cuốn{catText} từ khoảng {min:N0}đ:",
            (null, not null) => $"Mình gợi ý vài cuốn{catText} dưới {max:N0}đ:",
            _ => $"Mình gợi ý vài cuốn{catText} phù hợp:"
        };

        return new ChatBotRes(
            Text: textOut,
            Intent: intent,
            Confidence: conf,
            Books: top.Select(b => new {
                b.BookId,
                b.Title,
                b.Price,
                b.Images
            })
        );
    }

    private async Task<ChatBotRes> HandleRefine(string text, string intent, float conf)
    {
        // refine như lượt recommend có thêm range
        return await HandleRecommend(text, intent, conf);
    }

    private async Task<ChatBotRes> HandleAddToCart(string text, string intent, float conf)
    {
        var qty = IntentHelper.ExtractQuantity(text, 1);
        var kw  = IntentHelper.BuildKeywordForAddToCart(text);

        var results = await bookService.Search(kw, page: 1, pageSize: 20);
        var best = results.FirstOrDefault();
        if (best is null)
        {
            return new ChatBotRes($"Mình chưa tìm ra sách trùng khớp để thêm giỏ. Bạn mô tả rõ tên sách hơn nhé?", intent, conf);
        }
        
        var action = new BotAction(
            Type: "AddToCart",
            Payload: new { bookId = best.BookId, quantity = qty }
        );

        var reply = $"Mình đã chuẩn bị thêm **{qty} x {best.Title}** vào giỏ. Xác nhận nhé?";
        return new ChatBotRes(
            Text: reply,
            Intent: intent,
            Confidence: conf,
            Books: [new
            {
                best.BookId, 
                best.Title, 
                best.Price, 
                best.Images
            }],
            Actions: [action]
        );
    }
}