using Scraper.ConcreteClasses;
using Scraper.Contracts;

namespace Scraper.Services;

public class JsonProductStorageService : IProductStorage
{
    private readonly ProductRepository productRepository;
    private readonly ProductSerializer productSerializer;
    
    private const string FileName = "Products.json";
    
    public JsonProductStorageService(ProductSerializer productSerializer)
    {
        this.productSerializer = productSerializer;
        productRepository = new ProductRepository(LoadProductsFromFile());
    }
    
    private List<Product> LoadProductsFromFile()
    {
        if (!File.Exists(FileName)) return new List<Product>();
        
        var jsonString = File.ReadAllText(FileName);
        return productSerializer.DeserializeProducts(jsonString);
    }
    
    public async Task<bool> Store(IProduct product)
    {
        productRepository.AddProduct((Product)product);
        var updatedProducts = productSerializer.SerializeProducts(productRepository.GetProducts());
        await File.WriteAllTextAsync(FileName, updatedProducts);
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
        await File.WriteAllTextAsync(FileName, updatedProducts);

        return product;
    }
}