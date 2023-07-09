using System.Text.RegularExpressions;
using Scraper.Contracts;
using Scraper.Scrapers;

namespace Scraper.Stores;

public class DirkStore : IStore
{
    public IStoreScraper Scraper { get; }
    
    public DirkStore(IEnumerable<IStoreScraper> scrapers)
    {
        Scraper = scrapers.FirstOrDefault(x => x is DirkScraper) ?? throw new InvalidOperationException();
    }
    public string StoreName => "Dirk";
    public string StoreNameShort => StoreName;

    public IEnumerable<Regex> StoreMatchRegex => new Regex[]
    {
        new(@"^(http:\/\/www\.dirk\.nl|https:\/\/www\.dirk\.nl|www\.dirk\.nl)")
    };
    public IEnumerable<Regex> ExtractProductIdRegex => new Regex[]
    {
        new(@"\/(\d+)$")
    };
}