using ShoppingBasket.Models;

namespace ShoppingBasket.Abstractions;

public interface IProductCatalog
{
    Product GetByName(string productName);
}
