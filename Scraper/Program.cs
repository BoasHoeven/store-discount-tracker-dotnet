using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scraper.ConcreteClasses;
using Scraper.Contracts;
using Scraper.Extensions;
using Scraper.Policies;
using Scraper.Services;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        services
            .AddStoreServices()
            .AddTransient<StoreMatcherService>()
            .AddTransient<StoreDiscountService>()
            .AddSingleton<AlbertHeijnRateLimitPolicy>()
            .AddTransient<IProductStorage, JsonProductStorageService>()
            .AddScoped<ProductSerializer>()
            .AddTransient<ProductService>()
            .AddTransient<UrlExtractorService>()
            .AddHttpClient("AlbertHeijnClient", c =>
                {
                    c.BaseAddress = new Uri("https://ah.nl");
                    c.Timeout = TimeSpan.FromMinutes(10);
                })
                .AddHttpMessageHandler<AlbertHeijnRateLimitPolicy>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));
    })
    .Build();

using var serviceScope = host.Services.CreateScope();
var services = serviceScope.ServiceProvider;

// Get Services
var productService = services.GetRequiredService<ProductService>();

// Add product
const string url = "https://www.ah.nl/producten/product/wi177570/lipton-ice-tea-sparkling-original";
await productService.AddProductFromMessage(url);

// Discounts
var discountService = services.GetRequiredService<StoreDiscountService>();
var discounts = await discountService.GetDiscounts();

Console.WriteLine("test");