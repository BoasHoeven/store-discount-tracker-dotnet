namespace Scraper.Contracts;

public interface IProductStorage
{
    Task<bool> Store(IProduct product);
    Task<bool> Exists(string productId, string storeName);
}