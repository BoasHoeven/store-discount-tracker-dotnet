using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Scrapers;

public class AlbertHeijnScraper : IProductScraper
{
    public async Task<IProduct?> GetProductFromId(string id)
    {
        IProduct placeholderProduct = new Product(id, "TestProduct", "Albert Heijn")
        {
            Price = 2,
            AddedBy = -1,
            LastUpdated = DateTime.UtcNow,
            Created = DateTime.UtcNow,
        };

        return placeholderProduct;
    }
}