using Scraper.ConcreteClasses;

namespace Scraper.Contracts;

public interface IStoreScraper
{
    Task<IProduct?> GetProductFromId(string id, long userId);
    Task<ProductDiscount?> IsOnDiscount(IProduct product);
}