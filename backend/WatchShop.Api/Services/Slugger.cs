using System.Globalization;
using System.Text;

namespace WatchShop.Api.Services;

public static class Slugger
{
    /// <summary>"Đồng hồ Seiko 5" -> "dong-ho-seiko-5"</summary>
    public static string Make(string input)
    {
        var normalized = input.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        var slug = sb.ToString().Trim('-');
        return slug.Length == 0 ? "item" : slug;
    }
}
