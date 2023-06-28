// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scraper.ConcreteClasses;
using Scraper.Contracts;
using Scraper.Extensions;
using Scraper.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddStoreServices()
    .AddSingleton<StoreMatcherService>()
    .AddSingleton<IProductStorage, JsonProductStorageService>()
    .AddSingleton<ProductSerializer>();

var services = builder.Build().Services;

// Get Services
var storeMatcher = services.GetRequiredService<StoreMatcherService>();
var productStorage = services.GetRequiredService<IProductStorage>();

// Determine store.
const string url = "https://www.ah.nl/producten/product/wi61758/optimel-drinkyoghurt-framboos";
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