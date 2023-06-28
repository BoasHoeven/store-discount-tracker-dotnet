namespace Scraper.Contracts;

public interface IProductStorage
{
    Task<bool> Store(IProduct product);
}