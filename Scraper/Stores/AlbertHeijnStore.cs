using System.Text.RegularExpressions;
using Scraper.Contracts;
using Scraper.Scrapers;

namespace Scraper.Stores;

public class AlbertHeijnStore : IStore
{
    private readonly IStoreScraper scraper;
    public AlbertHeijnStore(IEnumerable<IStoreScraper> scrapers)
    {
        scraper = scrapers.FirstOrDefault(x => x.GetType() == typeof(AlbertHeijnScraper)) ?? throw new InvalidOperationException();
    }
    
    public string StoreName => "Albert Heijn";
    public IStoreScraper GetProductScraper() => scraper;
    public IEnumerable<Regex> StoreMatchRegex => new Regex[]
    {
        new(@"^(http:\/\/www\.ah\.nl|https:\/\/www\.ah\.nl|www\.ah\.nl)")
    };
    public IEnumerable<Regex> ExtractProductIdRegex => new Regex[]
    {
        new(@"\/product\/wi(\d+)")
    };
}