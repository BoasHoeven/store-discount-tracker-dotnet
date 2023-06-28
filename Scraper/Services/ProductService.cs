using Scraper.Contracts;

namespace Scraper.Services;

public class ProductService
{
    private readonly StoreMatcherService storeMatcher;
    private readonly IProductStorage productStorage;
    private readonly UrlExtractorService urlExtractorService;

    public ProductService(StoreMatcherService storeMatcher, IProductStorage productStorage, UrlExtractorService urlExtractorService)
    {
        this.storeMatcher = storeMatcher;
        this.productStorage = productStorage;
        this.urlExtractorService = urlExtractorService;
    }

    public async Task AddProductFromMessage(string message)
    {
        var url = UrlExtractorService.ExtractUrlFromMessage(message);
        if(url is null)
        {
            Console.WriteLine("No valid URL was found in the message.");
            return;
        }
        
        var store = storeMatcher.GetStoreFromUrl(url);
        if(store is not null)
        {
            Console.WriteLine($"URL matches store: {store.StoreName}");
            var id = StoreMatcherService.GetProductIdFromUrl(store, url);
            Console.WriteLine($"Id extracted from url: {id}");
            if (id is null)
                return;

            var productScraper = store.GetProductScraper();
            var product = await productScraper.GetProductFromId(id);
            if (product is null)
                return;

            await productStorage.Store(product);
        }
        else
        {
            Console.WriteLine("URL does not match any store");
        }
    }
}
