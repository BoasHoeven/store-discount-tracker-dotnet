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
        var existingProduct = productRepository.FindProductByIdAndStore(product.Id, product.StoreName);
        if (existingProduct is not null)
        {
            Console.WriteLine($"Product with ID {product.Id} from {product.StoreName} already exists in storage. Ignoring.");
            return false;
        }
        
        productRepository.AddProduct((Product)product);
        var updatedProducts = productSerializer.SerializeProducts(productRepository.GetProducts());

        await File.WriteAllTextAsync(FileName, updatedProducts);

        return true;
    }
}