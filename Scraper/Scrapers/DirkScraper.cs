using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Scrapers;

public class DirkScraper : IProductScraper
{
    
    public async Task<IProduct?> GetProductFromId(string id)
    {
        IProduct placeholderProduct = new Product(id, "test", "Dirk")
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