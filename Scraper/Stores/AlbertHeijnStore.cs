using System.Text.RegularExpressions;
using Scraper.Contracts;
using Scraper.Scrapers;

namespace Scraper.Stores;

public sealed class AlbertHeijnStore : IStore
{
    public IStoreScraper Scraper { get; }
    
    public AlbertHeijnStore(IEnumerable<IStoreScraper> scrapers)
    {
        Scraper = scrapers.FirstOrDefault(x => x is AlbertHeijnScraper) ?? throw new InvalidOperationException();
    }
    public string StoreName => "Albert Heijn";
    public string StoreNameShort => "Ah";

    public IEnumerable<Regex> StoreMatchRegex => new Regex[]
    {
        new(@"^(http:\/\/|https:\/\/)?(www\.)?ah\.nl")
    };
    public IEnumerable<Regex> ExtractProductIdRegex => new Regex[]
    {
        new(@"\/product\/wi(\d+)")
    };
}