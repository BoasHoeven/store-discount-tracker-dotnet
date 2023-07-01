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
        var url = urlExtractorService.ExtractUrlFromMessage(message);
        if(url is null)
        {
            Console.WriteLine($"{message} did not contain a valid url.");
            return;
        }
        
        var store = storeMatcher.GetStoreFromUrl(url);
        if (store is null)
        {
            Console.WriteLine($"{url} did not match any store");
            return;
        }
        Console.WriteLine($"URL matches store: {store.StoreName}");
        
        var id = StoreMatcherService.GetProductIdFromUrl(store, url);
        Console.WriteLine($"{id} was extracted from url: {url}");
        if (id is null)
            return;
        
        if (await productStorage.Exists(id, store.StoreName))
        {
            Console.WriteLine($"Product with ID {id} from {store.StoreName} already exists in storage.");
            return;
        }
        
        var product = await store.Scraper.GetProductFromId(id);
        if (product is null)
            return;

        await productStorage.Store(product);
        Console.WriteLine("Product with ID {id} has been added.");
    }

    public IEnumerable<IProduct> GetAllProducts()
    {
        return productStorage.GetAllProducts();
    }
}
