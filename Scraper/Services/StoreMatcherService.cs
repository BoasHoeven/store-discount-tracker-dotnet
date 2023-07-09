using Scraper.Contracts;

namespace Scraper.Services;

public sealed class StoreMatcherService
{
    private readonly IEnumerable<IStore> stores;

    public StoreMatcherService(IEnumerable<IStore> stores)
    {
        this.stores = stores;
    }
    
    public IStore? GetStoreFromUrl(string url) =>
        stores.FirstOrDefault(store => store.StoreMatchRegex.Any(regex => regex.IsMatch(url)));

    public static string? GetIdFromUrl(IStore store, string url) => store.ExtractProductIdRegex
        .Select(regex => regex.Match(url))
        .Where(match => match.Success)
        .Select(match => match.Groups[1].Value).FirstOrDefault();

    public IStore? GetStoreFromProduct(IProduct product) =>
        stores.FirstOrDefault(x => x.StoreName == product.StoreName);
}