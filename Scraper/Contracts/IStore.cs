using System.Text.RegularExpressions;

namespace Scraper.Contracts;

public interface IStore
{
    IStoreScraper Scraper { get; }
    string StoreName { get; }
    string StoreNameShort { get; }
    IEnumerable<Regex> StoreMatchRegex { get; }
    IEnumerable<Regex> ExtractProductIdRegex { get; }
}