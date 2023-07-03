namespace Scraper.Contracts;

public interface IProductStorage
{
    Task<bool> Store(IProduct product);
    Task<IProduct?> Exists(string productId, string storeName);
    IEnumerable<IProduct> GetAllProducts();
    IEnumerable<IProduct> GetProductsByName(string message);
    Task<IProduct?> RemoveProduct(string productId, string storeName);
}