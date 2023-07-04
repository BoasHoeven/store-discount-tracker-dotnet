namespace Scraper.Contracts;

public interface IProduct
{
    string Id { get; }
    string StoreName { get; }
    string Name { get; }
    string UnitSize { get; set; }
    decimal Price { get; set; }
    DateTime Created { get; init; }
    DateTime LastUpdated { get; set; }
    long AddedBy { get; init; }
}