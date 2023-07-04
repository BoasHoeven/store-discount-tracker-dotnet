using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scraper.Extensions;
using Scraper.Services;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        services.AddScraperServices();
    })
    .Build();

using var serviceScope = host.Services.CreateScope();
var services = serviceScope.ServiceProvider;

// Get Services
var productService = services.GetRequiredService<ProductService>();

// Add product
const string message = "https://www.dirk.nl/aanbiedingen/almhof-room-of-fruityoghurt/97481/almhof-roomyoghurt-aardbei/8589";
// const string message = "https://www.ah.nl/producten/product/wi2800/coca-cola-regular";
await productService.AddProductFromMessage(message, -1);

// Discounts
var discountService = services.GetRequiredService<StoreDiscountService>();
var discounts = await discountService.GetDiscounts();

Console.WriteLine("test");