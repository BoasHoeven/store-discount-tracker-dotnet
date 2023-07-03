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
const string message = "https://www.ah.nl/producten/product/wi169797/ah-avocado";
// const string message = "https://www.ah.nl/producten/product/wi2800/coca-cola-regular";
await productService.AddProductFromMessage(message);

// Discounts
var discountService = services.GetRequiredService<StoreDiscountService>();
var discounts = await discountService.GetDiscounts();

Console.WriteLine("test");