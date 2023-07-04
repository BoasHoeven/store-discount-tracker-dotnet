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

    public async Task<string> AddProductFromMessage(string message)
    {
        var url = urlExtractorService.ExtractUrlFromMessage(message);
        if (url is null)
        {
            return $"{message} did not contain a valid url.";
        }
    
        var store = storeMatcher.GetStoreFromUrl(url);
        if (store is null)
        {
            return $"{url} did not match any store";
        }
    
        var id = StoreMatcherService.GetIdFromUrl(store, url);
        if (id is null)
            return $"No product id found in the url: {url}";
        
        var existingProduct = await productStorage.Exists(id, store.StoreName);
        if (existingProduct is not null)
        {
            return $"*{existingProduct}* from *{store.StoreName}* has already been added.";
        }
    
        var product = await store.Scraper.GetProductFromId(id);
        if (product is null)
            return $"Could not scrape a product with id: {id} from store: {store.StoreName}";
    
        await productStorage.Store(product);
        return $"Product {product} has been added.";
    }

    public IEnumerable<IProduct> GetAllProducts()
    {
        return productStorage.GetAllProducts();
    }

    public IEnumerable<IProduct> GetProductsByName(string message)
    {
        return productStorage.GetProductsByName(message);
    }

    public Task<IProduct?> RemoveProduct(string productId, string storeName)
    {
        return productStorage.RemoveProduct(productId, storeName);
    }
}
