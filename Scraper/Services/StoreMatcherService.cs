using Scraper.Contracts;

namespace Scraper.Services;

public class StoreMatcherService
{
    private readonly IEnumerable<IStore> stores;

    public StoreMatcherService(IEnumerable<IStore> stores)
    {
        this.stores = stores;
    }
    
    public IStore? GetStoreFromUrl(string url) =>
        stores.FirstOrDefault(store => store.StoreMatchRegex.Any(regex => regex.IsMatch(url)));

    public static string? GetProductIdFromUrl(IStore store, string url) => store.ExtractProductIdRegex
        .Select(regex => regex.Match(url))
        .Where(match => match.Success)
        .Select(match => match.Groups[1].Value).FirstOrDefault();
}