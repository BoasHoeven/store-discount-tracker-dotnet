using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ProductMonitoringService.ConcreteClasses;
using ProductMonitoringService.Contracts;

namespace ProductMonitoringService.Scrapers;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class AlbertHeijnScraper : IStoreScraper
{
    private readonly HttpClient client;
    private readonly IMemoryCache cache;
    private readonly ILogger<AlbertHeijnScraper> logger;
    private readonly StringBuilder cookieBuilder = new();

    public AlbertHeijnScraper(IHttpClientFactory clientFactory, IMemoryCache cache, ILogger<AlbertHeijnScraper> logger)
    {
        ArgumentNullException.ThrowIfNull(clientFactory);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        this.cache = cache;
        this.logger = logger;
        client = clientFactory.CreateClient("AlbertHeijnClient");
    }

    private static string CookieStripper(string cookie)
    {
        return $"{cookie.Split(';').First()};";
    }

    private async Task EnsureCookiesAsync()
    {
        if (cache.TryGetValue("CookieData", out string cachedCookie))
        {
            cookieBuilder.Clear();
            cookieBuilder.Append(cachedCookie);
            return;
        }

        var response = await client.GetAsync("https://www.ah.nl/");
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var cookie = cookies.First(x => x.Contains("Ahonlnl-Prd-01"));
            cookieBuilder.Clear();
            // for (var i = 7; i <= 10; i++)
            // {
            //     var cookie = CookieStripper(cookies.ElementAt(i));
            //     cookieBuilder.Append(cookie);
            // }
            cookieBuilder.Append(cookie);

            cache.Set("CookieData", cookieBuilder, TimeSpan.FromMinutes(5));
            logger.LogInformation("Cookies initialized: {Cookies}", cookieBuilder);
        }
        else
        {
            logger.LogWarning("Failed to initialize cookies. StatusCode: {StatusCode}", response.StatusCode);
        }
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

        if (cache.TryGetValue($"Discount_{product.Id}", out ProductDiscount? discountCache))
        {
            logger.LogInformation("Cache hit for {Product}", product);
            return discountCache;
        }

        var productDetails = await GetProductDetailsFromId(product.Id);
        if (productDetails?.discount is null) return null;

        var discount = productDetails.discount;
        var tieredOffer = discount.tieredOffer?.Length > 0 ? string.Join(" & ", discount.tieredOffer): "";
        var discountMessage = tieredOffer != string.Empty ? tieredOffer : productDetails.shield?.text;

        var productDiscount = new ProductDiscount
        {
            Product = product,
            NewPrice = productDetails.price.now,
            OldPrice = productDetails.price.was,
            DiscountMessage = discountMessage ?? "",
            StartDate = discount.startDate,
            EndDate = discount.endDate
        };

        cache.Set($"Discount_{product.Id}", productDiscount, discount.endDate.ToUniversalTime());

        return productDiscount;
    }

    private async Task<ProductDetails?> GetProductDetailsFromId(string id)
    {
        await EnsureCookiesAsync();
        client.DefaultRequestHeaders.Add("Cookie", cookieBuilder.ToString());

        var response = await client.GetAsync($"zoeken/api/products/product?webshopId={id}");

        if (!response.IsSuccessStatusCode) return null;

        var jsonResponse = await response.Content.ReadAsStringAsync();

        var root = JsonSerializer.Deserialize<RootObject>(jsonResponse);
        var productDetails = root?.card.products.FirstOrDefault();

        return productDetails;
    }
}
