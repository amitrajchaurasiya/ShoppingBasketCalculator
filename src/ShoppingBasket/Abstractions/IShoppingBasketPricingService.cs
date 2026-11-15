namespace ShoppingBasket.Abstractions;

public interface IShoppingBasketPricingService
{
    int CalculateTotalPence(IEnumerable<string> items);
    decimal CalculateTotalPounds(IEnumerable<string> items);
}
