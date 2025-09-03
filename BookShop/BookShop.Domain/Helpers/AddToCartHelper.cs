using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BookShop.Domain.Helpers;

public record AddToCartItem(
    int? Index, 
    string? Title, 
    int Quantity
    );

public record AddToCartRequest(
    bool All,
    List<AddToCartItem> Items,
    int? GlobalEachQty
    );

public class AddToCartHelper
{
    private static readonly Dictionary<string,int> VietNums = new(StringComparer.OrdinalIgnoreCase){
        ["một"]=1,["mot"]=1,["hai"]=2,["ba"]=3,["bốn"]=4,["bon"]=4,
        ["năm"]=5,["nam"]=5,["sáu"]=6,["sau"]=6,["bảy"]=7,["bay"]=7,
        ["tám"]=8,["tam"]=8,["chín"]=9,["chin"]=9,["mười"]=10,["muoi"]=10
    };

    public static AddToCartRequest Parse(string text)
    {
        var norm = Normalize(text);
        var all = norm.Contains("tat ca") || norm.Contains("toan bo") ||
                  norm.Contains("het") || norm.Contains("nhung cuon vua roi") ||
                  norm.Contains("nhu tren") || norm.Contains("cuon tren");
        
        int? eachQty = TryFindGlobalEach(norm);

        var items = new List<AddToCartItem>();

        // index list: "cuốn 1, 3 và 5", "sách #2 x3"
        var idxRegex = new Regex(@"(?:sach|cuon|#)\s*(\d+)\s*(?:x\s*(\d+))?", RegexOptions.IgnoreCase);
        foreach (Match m in idxRegex.Matches(norm))
        {
            int idx = int.Parse(m.Groups[1].Value);
            int qty = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            items.Add(new AddToCartItem(Index: idx, Title: null, Quantity: qty));
        }

        // Tiêu đề trong ngoặc kép: ".*" x2
        var titleQuoted = new Regex("\"([^\"]+)\"\\s*(?:x\\s*(\\d+))?", RegexOptions.IgnoreCase);
        foreach (Match m in titleQuoted.Matches(text)) // giữ nguyên text gốc để match có dấu
        {
            var title = m.Groups[1].Value.Trim();
            int qty = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            items.Add(new AddToCartItem(Index: null, Title: title, Quantity: qty));
        }

        // “cuốn thứ hai/ba …”
        var thuRegex = new Regex(@"cuon thu\s+([^\s,\.]+)(?:\s*x\s*(\d+))?", RegexOptions.IgnoreCase);
        foreach (Match m in thuRegex.Matches(norm))
        {
            var word = m.Groups[1].Value;
            int idx = VietNums.TryGetValue(word, out var n) ? n : int.TryParse(word, out var ni) ? ni : -1;
            if (idx > 0)
            {
                int qty = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
                items.Add(new AddToCartItem(Index: idx, Title: null, Quantity: qty));
            }
        }

        // // “2 quyển cuốn 3” / “cuốn 3 2 quyển”
        // var aroundQty = new Regex(@"(?:moi )?(?:cuon|sach)?\s*(\d+)\s*(?:quyen|quy?n|ban)", RegexOptions.IgnoreCase);

        return new AddToCartRequest(All: all, Items: items, GlobalEachQty: eachQty);
    }

    static int? TryFindGlobalEach(string norm)
    {
        // “mỗi/moi … 2 (quyển|cuốn)”
        var r = new Regex(@"(moi|mỗi)\s*(?:cuon|sach)?\s*(\d+)", RegexOptions.IgnoreCase);
        var m = r.Match(norm);
        return m.Success ? int.Parse(m.Groups[2].Value) : null;
    }

    static string Normalize(string s)
    {
        var formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(char.ToLowerInvariant(ch));
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool FuzzyTitleMatch(string candidate, string targetTitle)
    {
        string n1 = Normalize(candidate), n2 = Normalize(targetTitle);
        return n2.Contains(n1) || n1.Contains(n2);
    }
}