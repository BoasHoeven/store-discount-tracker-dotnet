using System.Text.RegularExpressions;

namespace Scraper.Contracts;

public interface IStore
{
    IProductScraper GetProductScraper();
    string StoreName { get; }
    IEnumerable<Regex> StoreMatchRegex { get; }
    IEnumerable<Regex> ExtractProductIdRegex { get; }
}