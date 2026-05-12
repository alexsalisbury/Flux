namespace Flux.Services;

using System.Text.RegularExpressions;

public static class LinkParser
{
    private static readonly Regex Rx = new(@"\[\[([^\]]+)\]\]", RegexOptions.Compiled);

    public static List<string> Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new();
        return Rx.Matches(text)
                 .Select(m => m.Groups[1].Value.Trim())
                 .Where(s => s.Length > 0)
                 .Distinct(StringComparer.OrdinalIgnoreCase)
                 .ToList();
    }
}
