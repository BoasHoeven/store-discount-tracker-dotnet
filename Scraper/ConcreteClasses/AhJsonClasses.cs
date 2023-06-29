namespace Scraper.ConcreteClasses;

public abstract class RootObject
{
    public Card card { get; set; }
}

public abstract class Card
{
    public List<ProductDetails> products { get; set; }
}

public abstract class ProductDetails
{
    public string title { get; set; }
    public Price price { get; set; } 
}

public abstract class Price
{
    public decimal now { get; set; }
    public string unitSize { get; set; }
}
