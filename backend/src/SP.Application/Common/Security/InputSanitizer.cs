using System.Net;
using System.Text.RegularExpressions;

namespace SP.Application.Common.Security;

public static class InputSanitizer
{
    private static readonly Regex HtmlTagRegex = new("<.*?>", RegexOptions.Compiled);

    /// <summary>
    /// Encodes HTML tags in strings to safely prevent scripting execution (XSS defense) in rich text like chat.
    /// </summary>
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return WebUtility.HtmlEncode(input.Trim());
    }

    /// <summary>
    /// Removes HTML tags entirely from fields that should be plain text, like names or phone numbers.
    /// </summary>
    public static string SanitizePlain(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var stripped = HtmlTagRegex.Replace(input, string.Empty);
        return stripped.Trim();
    }
}
