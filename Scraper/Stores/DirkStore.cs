using System.Text.RegularExpressions;
using Scraper.Contracts;
using Scraper.Scrapers;

namespace Scraper.Stores;

public class DirkStore : IStore
{
    private readonly IStoreScraper scraper;
    
    public DirkStore(IEnumerable<IStoreScraper> scrapers)
    {
        scraper = scrapers.FirstOrDefault(x => x.GetType() == typeof(DirkScraper)) ?? throw new InvalidOperationException();
    }
    
    public string StoreName => "Dirk";
    public IStoreScraper GetProductScraper() => new DirkScraper();
    public IEnumerable<Regex> StoreMatchRegex => new Regex[]
    {
        new(@"^(http:\/\/www\.dirk\.nl|https:\/\/www\.dirk\.nl|www\.dirk\.nl)")
    };
    public IEnumerable<Regex> ExtractProductIdRegex => new Regex[]
    {
        new(@"\/(\d+)$")
    };
}