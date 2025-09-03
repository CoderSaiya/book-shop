using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BookShop.Domain.Helpers;

public static class IntentHelper
{
    private static readonly Regex RxMoney = new(@"(\d+)\s*(k|nghìn|nghin|ngàn|ngan)?", RegexOptions.IgnoreCase|RegexOptions.Compiled);
    private static readonly Regex RxQty = new(@"(?<!\d)(\d+)(?!\d)", RegexOptions.Compiled);

    private static readonly Regex RxQuoted =
        new("(?:\"([^\"]+)\")|(?:“([^”]+)”)|(?:‘([^’]+)’)|(?:'([^']+)')",
            RegexOptions.Compiled);

    private static readonly Regex RxQtyPhrase =
        new(@"\b(?:x|\*)\s*\d+\b|\b\d+\s*(?:quy[eê]n|cu[oô]n|b[aà]n|t[aâ]p|copy)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxIndexRef =
        new(@"\b(?:#|cu[oô]n|s[aá]ch)\s*#?\s*\d+\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxNonWord =
        new(@"[^\p{L}\p{N}\s]+", RegexOptions.Compiled);

    private static readonly Regex RxMultiSpace =
        new(@"\s+", RegexOptions.Compiled);
    
    private static readonly string[] StopTokens = new[]
    {
        // hành động
        "them","bo","vao","gio","bo vao gio","vao gio","add","mua","dat","order",
        "dua","cho vao","chon","lay","giup","giup minh","cho minh","cho em","xac nhan",
        // đơn vị
        "quyen","cuon","sach","ban","tap","volume","vol","copy",
        // từ nối/đệm
        "va","voi","hoac","hay","nua","tiep","di","nhe",
        // đại từ/phụ trợ thường gặp
        "toi","toi","minh","ban","em","anh","chi"
    };
    
    public static (decimal? min, decimal? max) ExtractPriceRange(string text)
    {
        // Bắt tất cả số, hiểu "k" là *1000
        var ms = RxMoney.Matches(text);
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
        var m = RxQty.Match(text);
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

    public static List<string> BuildKeywordForAddToCart(string text, int maxCandidates = 5)
    {
        var candidates = new List<string>();

        // Ưu tiên tiêu đề trong ngoặc kép (giữ nguyên dấu)
        foreach (Match m in RxQuoted.Matches(text))
        {
            var title = FirstNonEmpty(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value);
            if (!string.IsNullOrWhiteSpace(title))
                candidates.Add(title.Trim());
        }

        // Làm sạch trực tiếp trên chuỗi có dấu
        var cleaned = text.ToLowerInvariant();
        cleaned = RxQtyPhrase.Replace(cleaned, " ");
        cleaned = RxIndexRef.Replace(cleaned, " ");
        cleaned = RxNonWord.Replace(cleaned, " ");
        cleaned = RxMultiSpace.Replace(cleaned, " ").Trim();

        // Loại stopword bằng cách so khớp phiên bản "không dấu"
        var keptTokens = new List<string>();
        foreach (var tok in cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Regex.IsMatch(tok, @"^\d+$")) continue; // bỏ số lẻ
            var noAccent = RemoveDiacritics(tok);
            if (StopTokens.Contains(noAccent)) continue;
            keptTokens.Add(tok);  // giữ dấu
        }

        var main = string.Join(' ', keptTokens).Trim();
        if (!string.IsNullOrWhiteSpace(main))
            candidates.Add(main);

        // inh thêm n-gram
        for (int w = Math.Min(8, keptTokens.Count); w >= 3; w--)
        {
            for (int i = 0; i + w <= keptTokens.Count; i++)
            {
                var span = string.Join(' ', keptTokens.Skip(i).Take(w));
                if (candidates.Count >= maxCandidates) break;
                if (!string.IsNullOrWhiteSpace(span) &&
                    !candidates.Contains(span, StringComparer.OrdinalIgnoreCase))
                    candidates.Add(span);
            }
            if (candidates.Count >= maxCandidates) break;
        }

        // Duy nhất & cắt số lượng theo yêu cầu
        return candidates
            .Select(s => s.Trim())
            .Where(s => s.Length >= 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(maxCandidates)
            .ToList();
    }

    public static bool LooksLikeRecommend(string text)
    {
        var t = text.ToLowerInvariant();
        return t.Contains("gợi ý") || t.Contains("tư vấn") || t.Contains("đề xuất") || t.Contains("cho tôi") || t.Contains("cần mua") || t.Contains("muốn sách");
    }
    
    private static string FirstNonEmpty(params string[] xs)
    {
        foreach (var x in xs) if (!string.IsNullOrWhiteSpace(x)) return x;
        return string.Empty;
    }
}