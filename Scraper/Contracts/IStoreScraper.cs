using Scraper.ConcreteClasses;

namespace Scraper.Contracts;

public interface IStoreScraper
{
    Task<IProduct?> GetProductFromId(string id);
    Task<ProductDiscount?> IsOnDiscount(IProduct product);
}