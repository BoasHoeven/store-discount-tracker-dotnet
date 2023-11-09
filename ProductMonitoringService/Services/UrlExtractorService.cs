using System.Text.RegularExpressions;

namespace ProductMonitoringService.Services;

public partial class UrlExtractorService
{
    private static readonly Regex UrlRegex = MyRegex();

    public string? ExtractUrlFromMessage(string message)
    {
        var match = UrlRegex.Match(message);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"(http:\/\/|https:\/\/)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=\n]*)?", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
