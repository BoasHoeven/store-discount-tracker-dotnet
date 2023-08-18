using Scraper.Contracts;

namespace Scraper.ConcreteClasses;

public sealed class ProductRepository
{
    private readonly List<Product> products;

    public ProductRepository(List<Product> products)
    {
        this.products = products;
    }

    public Product? FindByIdAndStore(string id, string storeName)
    {
        return products.FirstOrDefault(p => p.Id == id && p.StoreName == storeName);
    }

    public void Add(Product product)
    {
        products.Add(product);
    }

    public IEnumerable<Product> GetAll()
    {
        return products;
    }

    public IEnumerable<IProduct> GetByName(string name)
    {
        return products.Where(p => p.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase));
    }

    public void Remove(Product product)
    {
        products.Remove(product);
    }

    public void SetProducts(IEnumerable<Product> newProducts)
    {
        products.Clear();
        products.AddRange(newProducts);
    }
}
