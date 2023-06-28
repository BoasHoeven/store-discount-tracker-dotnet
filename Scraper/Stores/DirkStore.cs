using System.Text.RegularExpressions;
using Scraper.Contracts;
using Scraper.Scrapers;

namespace Scraper.Stores;

public class DirkStore : IStore
{
    public string StoreName => "Dirk";
    public IProductScraper GetProductScraper() => new DirkScraper();
    public IEnumerable<Regex> StoreMatchRegex => new Regex[]
    {
        new(@"^(http:\/\/www\.dirk\.nl|https:\/\/www\.dirk\.nl|www\.dirk\.nl)")
    };
    public IEnumerable<Regex> ExtractProductIdRegex => new Regex[]
    {
        new(@"\/(\d+)$")
    };
}