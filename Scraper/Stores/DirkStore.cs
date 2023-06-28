using System.Text.RegularExpressions;
using Scraper.Contracts;

namespace Scraper.Stores;

public class DirkStore : IStore
{
    public IProductScraper GetProductScraper()
    {
        throw new NotImplementedException();
    }
    public string StoreName => "Dirk";
    public IEnumerable<Regex> StoreMatchRegex => new Regex[]
    {
        new(@"^(http:\/\/www\.dirk\.nl|https:\/\/www\.dirk\.nl|www\.dirk\.nl)")
    };
    public IEnumerable<Regex> ExtractProductIdRegex => new Regex[]
    {
        new(@"\/(\d+)$")
    };
}