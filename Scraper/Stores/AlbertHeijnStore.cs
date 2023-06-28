using System.Text.RegularExpressions;
using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Stores;

public class AlbertHeijnStore : IStore
{
    public string StoreName => "Albert Heijn";
    public IEnumerable<Regex> StoreMatchRegex => new Regex[]
    {
        new(@"^(http:\/\/www\.ah\.nl|https:\/\/www\.ah\.nl|www\.ah\.nl)")
    };
    public IEnumerable<Regex> ExtractProductIdRegex => new Regex[]
    {
        new(@"\/product\/wi(\d+)")
    };
    public IProductScraper GetProductScraper()
    {
        return new AlbertHeijnProductScraper();
    }
}