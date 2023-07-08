using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Services;

public class JsonProductStorageService : IProductStorage
{
    private readonly ProductRepository productRepository;
    private readonly ProductSerializer productSerializer;
    private readonly string filePath;
    
    public JsonProductStorageService(ProductSerializer productSerializer)
    {
        this.productSerializer = productSerializer;
        filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.Parent!.FullName,"Products.json");

        productRepository = new ProductRepository(LoadProductsFromFile());
    }
    
    private List<Product> LoadProductsFromFile()
    {
        if (!File.Exists(filePath)) return new List<Product>();
        
        var jsonString = File.ReadAllText(filePath);
        return productSerializer.DeserializeProducts(jsonString);
    }
    
    public async Task<bool> Store(IProduct product)
    {
        productRepository.AddProduct((Product)product);
        var updatedProducts = productSerializer.SerializeProducts(productRepository.GetProducts());
        await File.WriteAllTextAsync(filePath, updatedProducts);
        return true;
    }

    public Task<IProduct?> Exists(string productId, string storeName)
    {
        var existingProduct = productRepository.FindProductByIdAndStore(productId, storeName);
        
        return Task.FromResult<IProduct>(existingProduct);
    }

    public IEnumerable<IProduct> GetAllProducts() => productRepository.GetProducts();
    public IEnumerable<IProduct> GetProductsByName(string message)
    {
        return productRepository.GetProductsByName(message);
    }

    public async Task<IProduct?> RemoveProduct(string productId, string storeName)
    {
        var product = productRepository.FindProductByIdAndStore(productId, storeName);

        if (product is null) return null;
        productRepository.RemoveProduct(product);
        var updatedProducts = productSerializer.SerializeProducts(productRepository.GetProducts());
        await File.WriteAllTextAsync(filePath, updatedProducts);

        return product;
    }
}