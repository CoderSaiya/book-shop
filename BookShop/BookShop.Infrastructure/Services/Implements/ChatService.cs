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
    ISessionMemory memory,
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
            conf = Math.Max(conf, 0.55f);
        }

        switch (intent)
        {
            case "recommend":
                return await HandleRecommend(sessionId, userMessage, intent, conf);

            case "refine":
                return await HandleRefine(sessionId, userMessage, intent, conf);

            case "add_to_cart":
                return await HandleAddToCart(sessionId, userMessage, intent, conf);

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
                return await HandleRecommend(sessionId, userMessage, "recommend", conf * 0.8f);
        }
    }

    private async Task<ChatBotRes> HandleRecommend(Guid sessionId, string text, string intent, float conf)
    {
        var (min, max) = IntentHelper.ExtractPriceRange(text);
        Console.Write("min:" + max + " max: " + max);
        
        var catNames = IntentHelper.ExtractCategoryNames(text);
        var catMaps  = (catNames.Count > 0) ? await categoryService.MapNamesToIdsAsync(catNames) : [];
        
        List<BookRes> top;
        bool exactFound;

        if (catMaps.Count > 0)
        {
            // gọi service theo category
            top = (await bookService.RecommendByCategories(
                    categoryIds: catMaps.Select(c => c.Id),
                    limit: 8,
                    minPrice: min, maxPrice: max,
                    keyword: ""))
                .ToList();
            
            exactFound = top.Count > 0;
        }
        else
        {
            // fallback: logic cũ theo keyword
            var results = await bookService.Search(page: 1, pageSize: 40);
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
            exactFound = top.Count > 0;
        }

        if (!exactFound)
        {
            var suggest = await bookService.GetTrendingAsync(days: 30, limit: 8);
            memory.SaveRecommendations(sessionId, suggest);
            
            string catText = (catMaps.Count > 0)
                ? $" theo thể loại {string.Join(", ", catMaps.Select(c => c.Name))}"
                : "";

            string missText = (min, max) switch
            {
                (not null, not null) => $"Mình không tìm thấy sách{catText} trong tầm giá {min:N0}–{max:N0}đ.",
                (not null, null) => $"Mình không tìm thấy sách{catText} từ khoảng {min:N0}đ trở lên.",
                (null, not null) => $"Mình không tìm thấy sách{catText} dưới {max:N0}đ.",
                _ => $"Mình chưa tìm thấy sách phù hợp{catText}."
            };

            return new ChatBotRes(
                Text: missText + " Bạn có thể tham khảo những cuốn phổ biến sau:",
                Intent: intent,
                Confidence: conf,
                Books: suggest.Select(b => new { b.BookId, b.Title, b.Price, b.Images })
            );
        }
        
        memory.SaveRecommendations(sessionId, top);

        // build câu trả lời
        string catText2 = (catMaps.Count > 0)
            ? $" theo thể loại {string.Join(", ", catMaps.Select(c => c.Name))}"
            : "";

        var textOut = (min, max) switch
        {
            (not null, not null) => $"Mình gợi ý vài cuốn{catText2} trong khoảng {min:N0}–{max:N0}đ:",
            (not null, null) => $"Mình gợi ý vài cuốn{catText2} từ khoảng {min:N0}đ:",
            (null, not null) => $"Mình gợi ý vài cuốn{catText2} dưới {max:N0}đ:",
            _ => $"Mình gợi ý vài cuốn{catText2} phù hợp:"
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

    private async Task<ChatBotRes> HandleRefine(Guid sessionId, string text, string intent, float conf)
    {
        // refine như lượt recommend có thêm range
        return await HandleRecommend(sessionId, text, intent, conf);
    }

    private async Task<ChatBotRes> HandleAddToCart(Guid sessionId, string text, string intent, float conf)
    {
        var recos = memory.GetRecommendations(sessionId);
        var req = AddToCartHelper.Parse(text);

        var chosen = new List<(BookRes Book, int Qty)>();

        if (req.All && recos.Count > 0)
        {
            int each = req.GlobalEachQty ?? 1;
            chosen.AddRange(recos.Select(b => (b, each)));
        }

        // match theo index
        foreach (var item in req.Items.Where(i => i.Index is not null))
        {
            var idx = item.Index!.Value;
            if (idx >= 1 && idx <= recos.Count)
            {
                var q = item.Quantity > 0 ? item.Quantity : (req.GlobalEachQty ?? 1);
                chosen.Add((recos[idx - 1], q));
            }
        }

        // match theo tiêu đề
        foreach (var item in req.Items.Where(i => i.Title is not null))
        {
            var found = recos.FirstOrDefault(b => AddToCartHelper.FuzzyTitleMatch(item.Title!, b.Title.Vi));
            if (found is not null)
            {
                var q = item.Quantity > 0 ? item.Quantity : (req.GlobalEachQty ?? 1);
                chosen.Add((found, q));
            }
        }

        // nếu vẫn chưa chọn được gì, fallback như cũ: search theo keyword suy luận
        if (chosen.Count == 0)
        {
            var qty = IntentHelper.ExtractQuantity(text, 1);
            var kws = IntentHelper.BuildKeywordForAddToCart(text);
            
            BookRes? best = null;
            foreach (var kw in kws)
            {
                Console.Write("ABC:" + kw);
                var hits = await bookService.Search(kw, page: 1, pageSize: 10);
                best = hits.FirstOrDefault();
                if (best is not null) break;
            }
            Console.Write(best is null);
            
            if (best is null)
                return new ChatBotRes($"Mình chưa tìm ra sách trùng khớp để thêm giỏ. Bạn mô tả rõ tên sách hơn nhé?", intent, conf);

            chosen.Add((best, qty));
        }

        // gộp trùng (nếu user nhắc tên/index 2 lần)
        var grouped = chosen
            .GroupBy(x => x.Book.BookId)
            .Select(g => (Book: g.First().Book, Qty: g.Sum(x => x.Qty)))
            .ToList();

        // tạo nhiều actions
        var actions = grouped.Select(g =>
            new BotAction("AddToCart", new { bookId = g.Book.BookId, quantity = g.Qty })
        ).ToList();

        var summary = string.Join(", ", grouped.Select(g => $"{g.Qty} × {g.Book.Title.Vi}"));
        var reply = $"Mình đã chuẩn bị thêm {grouped.Count} mặt hàng vào giỏ: {summary}. Xác nhận nhé?";

        return new ChatBotRes(
            Text: reply,
            Intent: intent,
            Confidence: conf,
            Books: grouped.Select(g => new { g.Book.BookId, g.Book.Title, g.Book.Price, g.Book.Images }),
            Actions: actions
        );
    }
}