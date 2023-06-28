using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scraper.ConcreteClasses;
using Scraper.Contracts;
using Scraper.Extensions;
using Scraper.Services;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        services
            .AddStoreServices()
            .AddSingleton<StoreMatcherService>()
            .AddSingleton<IProductStorage, JsonProductStorageService>()
            .AddSingleton<ProductSerializer>()
            .AddSingleton<ProductService>()
            .AddSingleton<UrlExtractorService>();
    })
    .Build();

using var serviceScope = host.Services.CreateScope();
var services = serviceScope.ServiceProvider;

// Get Services
var productService = services.GetRequiredService<ProductService>();

// Process product.
const string url = "https://www.ah.nl/producten/product/wi2800";
await productService.AddProductFromMessage(url);