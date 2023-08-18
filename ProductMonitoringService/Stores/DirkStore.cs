using System.Text.RegularExpressions;
using ProductMonitoringService.Contracts;
using ProductMonitoringService.Scrapers;

namespace ProductMonitoringService.Stores;

public sealed class DirkStore : IStore
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
        new(@"^(http:\/\/|https:\/\/)?(www\.)?dirk\.nl")
    };
    public IEnumerable<Regex> ExtractProductIdRegex => new Regex[]
    {
        new(@"\/(\d+)$")
    };
}