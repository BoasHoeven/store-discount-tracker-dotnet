using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Scrapers;

public class DirkScraper : IStoreScraper
{
    public async Task<IProduct?> GetProductFromId(string id)
    {
        IProduct placeholderProduct = new Product(id, "test", "1 L", "Dirk")
        {
            AddedBy = -1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
            Name = "TestProduct",
            Price = 3
        };

        return placeholderProduct;
    }
}