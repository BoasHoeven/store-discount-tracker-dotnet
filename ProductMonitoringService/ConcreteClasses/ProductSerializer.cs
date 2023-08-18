using System.Text.Json;

namespace Scraper.ConcreteClasses;

public sealed class ProductSerializer
{
    public static List<Product> DeserializeProducts(string jsonString)
    {
        return JsonSerializer.Deserialize<List<Product>>(jsonString) ?? new List<Product>();
    }

    public static string SerializeProducts(IEnumerable<Product> products)
    {
        return JsonSerializer.Serialize(products);
    }
}
