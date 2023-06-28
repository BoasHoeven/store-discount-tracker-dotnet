using System.Text.RegularExpressions;

namespace Scraper.Services;

public class UrlExtractorService
{
    private static readonly Regex UrlRegex = new(@"(http:\/\/|https:\/\/)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=\n]*)?", RegexOptions.Compiled);
    
    public static string? ExtractUrlFromMessage(string message)
    {
        var match = UrlRegex.Match(message);
        return match.Success ? match.Value : null;
    }
}
