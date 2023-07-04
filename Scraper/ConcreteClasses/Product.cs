using Scraper.Contracts;

namespace Scraper.ConcreteClasses;

public class Product : IProduct
{
    public string Id { get; init; }
    public string StoreName { get; init; }
    public string Name { get; init; }
    
    private decimal price;
    public decimal Price
    {
        get => price;
        set
        {
            price = value;
            LastUpdated = DateTime.UtcNow;
        }
    }

    private string unitSize;
    public string UnitSize
    {
        get => unitSize;
        set
        {
            unitSize = value;
            LastUpdated = DateTime.UtcNow;
        }
    }

    public DateTime Created { get; init; }
    public DateTime LastUpdated { get; set; }
    public long AddedBy { get; init; }

    public Product(string id, string name, string unitSize, string storeName)
    {
        Id = id;
        Name = name;
        StoreName = storeName;
        UnitSize = unitSize;
        Created = DateTime.UtcNow;
    }
    
    public override string ToString()
    {
        return $"{Name} ({UnitSize})";
    }
}