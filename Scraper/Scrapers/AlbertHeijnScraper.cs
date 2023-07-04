using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Scrapers;

public sealed class AlbertHeijnScraper : IStoreScraper
{
    private readonly HttpClient client;
    private readonly IMemoryCache cache;
    private readonly ILogger<AlbertHeijnScraper> logger;

    private readonly TimeZoneInfo dutchTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    public AlbertHeijnScraper(IHttpClientFactory clientFactory, IMemoryCache cache, ILogger<AlbertHeijnScraper> logger)
    {
        ArgumentNullException.ThrowIfNull(clientFactory);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        this.cache = cache;
        this.logger = logger;
        client = clientFactory.CreateClient("AlbertHeijnClient");
    }

    public async Task<IProduct?> GetProductFromId(string id, long userId)
    {
        var productDetails = await GetProductDetailsFromId(id);

        if (productDetails == null) return null;
        
        IProduct product = new Product(id, productDetails.title, productDetails.price.unitSize,"Albert Heijn")
        {
            Price = productDetails.price.was ?? productDetails.price.now,
            AddedBy = userId,
            LastUpdated = DateTime.UtcNow
        };

        return product;
    }

    public async Task<ProductDiscount?> IsOnDiscount(IProduct product)
    {
        if (product.StoreName != "Albert Heijn")
        {
            throw new ArgumentException($"Product and store mismatch. Product: {product.StoreName} and Store: Albert Heijn");
        }

        // Check cache first
        if (cache.TryGetValue($"Discount_{product.Id}", out ProductDiscount discountCache))
        {
            logger.LogInformation("Cache hit for {Product}", product);
            return discountCache;
        }

        var productDetails = await GetProductDetailsFromId(product.Id);
        if (productDetails?.discount is null) return null;

        var discount = productDetails.discount;
        var dutchTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dutchTimeZone);
        var isOnDiscount = dutchTime >= discount.startDate && dutchTime <= discount.endDate;

        if (!isOnDiscount)
            return null;

        var productDiscount = new ProductDiscount
        {
            Product = product,
            NewPrice = productDetails.price.now,
            OldPrice = productDetails.price.was,
            DiscountMessage = productDetails.shield?.text ?? ""
        };

        // Save to cache
        cache.Set($"Discount_{product.Id}", productDiscount, discount.endDate.ToUniversalTime());

        return productDiscount;
    }

    private async Task<ProductDetails?> GetProductDetailsFromId(string id)
    {
        var response = await client.GetAsync($"zoeken/api/products/product?webshopId={id}");

        if (!response.IsSuccessStatusCode) return null;
        
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var root = JsonSerializer.Deserialize<RootObject>(jsonResponse);
        var productDetails = root?.card.products.FirstOrDefault();

        return productDetails;
    }
}
