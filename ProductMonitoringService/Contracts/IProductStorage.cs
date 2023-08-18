using Scraper.ConcreteClasses;

namespace Scraper.Contracts;

public interface IProductStorage
{
    Task<bool> Add(IProduct product);
    Task<IProduct?> Exists(string productId, string storeName);
    IEnumerable<IProduct> GetAll();
    IEnumerable<IProduct> GetByName(string message);
    Task<IProduct?> Remove(string productId, string storeName);
    Task<bool> ImportProducts(IEnumerable<Product> products);
}