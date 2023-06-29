using System.Text.Json;
using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Scrapers;

public class AlbertHeijnScraper : IStoreScraper
{
    private readonly HttpClient client;

    public AlbertHeijnScraper(IHttpClientFactory clientFactory)
    {
        ArgumentNullException.ThrowIfNull(clientFactory);
        
        client = clientFactory.CreateClient("AlbertHeijnClient");
    }
    
    public async Task<IProduct?> GetProductFromId(string id)
    {
        var response = await client.GetAsync($"zoeken/api/products/product?webshopId={id}");

        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync();

        var root = JsonSerializer.Deserialize<RootObject>(content);
        var productDetails = root?.card.products.FirstOrDefault();

        if (productDetails == null) return null;
        IProduct product = new Product(id, productDetails.title, productDetails.price.unitSize,"Albert Heijn")
        {
            Price = productDetails.price.now,
            AddedBy = -1,
            LastUpdated = DateTime.UtcNow
        };

        return product;

    }
}