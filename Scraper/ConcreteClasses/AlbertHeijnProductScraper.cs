using Scraper.Contracts;

namespace Scraper.ConcreteClasses;

public class AlbertHeijnProductScraper : IProductScraper
{
    public Task<IProduct?> GetProductFromId(string id)
    {
        IProduct demoProduct = new Product(id, "test", "Albert Heijn")
        {
            Price = 2,
            AddedBy = -1,
            LastUpdated = DateTime.UtcNow,
            Created = DateTime.UtcNow,
        };

        return Task.FromResult(demoProduct);
    }
}