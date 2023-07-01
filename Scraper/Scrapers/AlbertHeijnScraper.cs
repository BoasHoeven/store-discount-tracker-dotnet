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
        var productDetails = await GetProductDetailsFromId(id);

        if (productDetails == null) return null;
        
        IProduct product = new Product(id, productDetails.title, productDetails.price.unitSize,"Albert Heijn")
        {
            Price = productDetails.price.was ?? productDetails.price.now,
            AddedBy = -1,
            LastUpdated = DateTime.UtcNow
        };

        return product;
    }

    public async Task<ProductDiscount?> IsOnDiscount(IProduct product)
    {
        if (product.StoreName != "Albert Heijn")
        {
            throw new ArgumentException("Product of another store was used");
        }

        var productDetails = await GetProductDetailsFromId(product.Id);
        if (productDetails?.discount is null) return null;

        var discount = productDetails.discount;
        var dutchTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        var dutchTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dutchTimeZone);
        var isOnDiscount = dutchTime >= discount.startDate && dutchTime <= discount.endDate;

        if (!isOnDiscount)
            return null;

        return new ProductDiscount
        {
            Product = product,
            NewPrice = productDetails.price.now,
            OldPrice = productDetails.price.was,
            TypeOfDiscount = productDetails.shield?.text ?? ""
        };
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
