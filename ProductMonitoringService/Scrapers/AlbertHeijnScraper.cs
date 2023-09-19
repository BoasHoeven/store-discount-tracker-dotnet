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

    private async Task EnsureCookiesAsync()
    {
        if (cache.TryGetValue("CookieData", out string? cachedCookie))
        {
            cookieBuilder.Clear();
            cookieBuilder.Append(cachedCookie);
            return;
        }

        var response = await client.GetAsync("https://www.ah.nl/");
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            cookieBuilder.Clear();
            var cookie = cookies.First(x => x.Contains("Ahonlnl-Prd-01"));
            cookieBuilder.Append(cookie);

            cache.Set("CookieData", cookieBuilder.ToString(), TimeSpan.FromMinutes(30));
            logger.LogInformation("Cookies initialized: {Cookies}", cookieBuilder.ToString());
        }
        else
        {
            logger.LogWarning("Failed to initialize cookies. StatusCode: {StatusCode}", response.StatusCode);
        }
    }

    public async Task<IProduct?> GetProductFromId(string id, long userId)
    {
        var productResponse = await GetProductResponse(new Product(id, "", "", ""));

        if (productResponse.ProductDetails == null)
            return null;

        var productDetails = productResponse.ProductDetails;

        IProduct product = new Product(id, productDetails.title, productDetails.price.unitSize,"Albert Heijn")
        {
            Price = productDetails.price.was ?? productDetails.price.now,
            AddedBy = userId,
            LastUpdated = DateTime.UtcNow
        };

        return product;
    }

    public async Task<ProductDiscountResponse> IsOnDiscount(IProduct product)
    {
        if (product.StoreName != "Albert Heijn")
        {
            throw new ArgumentException($"Product and store mismatch. Product: {product.StoreName} and Store: Albert Heijn");
        }

        if (cache.TryGetValue($"Discount_{product.Id}", out ProductDiscount? discountCache))
        {
            logger.LogInformation("Cache hit for {Product}", product);
            return new ProductDiscountResponse(new ProductResponse(product) { IsCached = true })
            {
                ProductDiscount = discountCache
            };
        }

        var productResponse = await GetProductResponse(product);
        if (productResponse.ProductDetails?.discount is null)
        {
            return new ProductDiscountResponse(productResponse);
        }

        var productDetails = productResponse.ProductDetails;
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

        return new ProductDiscountResponse(productResponse)
        {
            ProductDiscount = productDiscount
        };
    }

    private async Task<ProductResponse> GetProductResponse(IProduct product)
    {
        await EnsureCookiesAsync();
        client.DefaultRequestHeaders.Add("Cookie", cookieBuilder.ToString());

        var response = await client.GetAsync($"zoeken/api/products/product?webshopId={product.Id}");

        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            if (cookies.Any())
            {
                cache.Set("CookieData", cookies.Last(), TimeSpan.FromMinutes(30));
            }
            else
            {
                logger.LogError("There were no cookies to set!");
            }
        }

        if (!response.IsSuccessStatusCode)
            return new ProductResponse(product)
            {
                StatusCode = response.StatusCode
            };

        var jsonResponse = await response.Content.ReadAsStringAsync();

        var root = JsonSerializer.Deserialize<RootObject>(jsonResponse);
        var productDetails = root?.card.products.FirstOrDefault();

        return new ProductResponse(product)
        {
            StatusCode = response.StatusCode,
            ProductDetails = productDetails
        };
    }
}
