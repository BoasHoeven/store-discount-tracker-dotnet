using System.Text.RegularExpressions;

namespace Scraper.Contracts;

public interface IStore
{
    string StoreName { get; }
    IProductScraper GetProductScraper();
    IEnumerable<Regex> StoreMatchRegex { get; }
    IEnumerable<Regex> ExtractProductIdRegex { get; }
}