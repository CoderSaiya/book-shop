using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BookShop.Domain.Helpers;

public static class IntentHelper
{
    private static readonly Regex _rxMoney = new(@"(\d+)\s*(k|nghìn|nghin|ngàn|ngan)?", RegexOptions.IgnoreCase|RegexOptions.Compiled);
    private static readonly Regex _rxQty   = new(@"(?<!\d)(\d+)(?!\d)", RegexOptions.Compiled);

    public static (decimal? min, decimal? max) ExtractPriceRange(string text)
    {
        // Bắt tất cả số, hiểu "k" là *1000
        var ms = _rxMoney.Matches(text);
        if (ms.Count == 0) return (null, null);

        var vals = new List<decimal>();
        foreach (Match m in ms)
        {
            if (!int.TryParse(m.Groups[1].Value, out var v)) continue;
            var unit = m.Groups[2].Value.ToLowerInvariant();
            if (unit is "k" or "nghìn" or "nghin" or "ngàn" or "ngan")
                vals.Add(v * 1000m);
            else
                vals.Add(v); // nếu người dùng gõ "100000"
        }
        if (vals.Count == 1) return (vals[0] * 0.8m, vals[0] * 1.2m); // ±20%
        vals.Sort();
        return (vals.First(), vals.Last());
    }

    public static int ExtractQuantity(string text, int defaultQty = 1)
    {
        var m = _rxQty.Match(text);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var q))
            return Math.Clamp(q, 1, 50);
        return defaultQty;
    }
    
    public static List<string> ExtractCategoryNames(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        string t = RemoveDiacritics(text).ToLowerInvariant();

        var names = new List<string>();

        // #hashtag: #kinhdoanh #selfhelp #thieunhi ...
        var tagMatches = Regex.Matches(t, @"#([a-z0-9\-]+)", RegexOptions.Compiled);
        foreach (Match m in tagMatches)
            names.Add(m.Groups[1].Value);

        // các cụm phổ biến: "the loai <...>", "sach <...>"
        // ví dụ: "the loai kinh doanh", "sach tam ly", "thu vien van hoc"
        var patterns = new[]
        {
            @"\bthe\s*loai\s+([a-z0-9\s\-]+)",
            @"\bsach\s+([a-z0-9\s\-]+)",
            @"\bthu\s*vien\s+([a-z0-9\s\-]+)",
            @"\bgenre\s*[:\-]?\s*([a-z0-9\s\-]+)"
        };

        foreach (var p in patterns)
        {
            var m = Regex.Match(t, p, RegexOptions.Compiled);
            if (m.Success)
            {
                var raw = m.Groups[1].Value.Trim();
                // cắt theo dấu phẩy / và / hoặc "va"
                foreach (var part in Regex.Split(raw, @"(,|/|\s+va\s+)"))
                {
                    var s = part.Trim();
                    if (s.Length >= 3 && Regex.IsMatch(s, @"[a-z0-9]")) names.Add(s);
                }
            }
        }

        // loại trùng
        return names.Distinct().Take(5).ToList();
    }

    public static string RemoveDiacritics(string s)
    {
        var stFormD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in stFormD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string BuildKeywordForAddToCart(string text)
    {
        // loại bớt stop-phrases phổ biến
        var lowered = text.ToLowerInvariant();
        var stopPhrases = new[]
        {
            "thêm", "mua", "bỏ vào giỏ", "vào giỏ", "add", "cho mình", "giúp mình", "giúp", "1 quyển", "1 cuốn",
            "2 quyển", "2 cuốn", "3 quyển", "3 cuốn", "quyển", "cuốn", "tập", "bản", "sách"
        };
        foreach (var s in stopPhrases)
            lowered = lowered.Replace(s, "");

        return lowered.Trim();
    }

    public static bool LooksLikeRecommend(string text)
    {
        var t = text.ToLowerInvariant();
        return t.Contains("gợi ý") || t.Contains("tư vấn") || t.Contains("đề xuất") || t.Contains("cho tôi") || t.Contains("cần mua") || t.Contains("muốn sách");
    }
}