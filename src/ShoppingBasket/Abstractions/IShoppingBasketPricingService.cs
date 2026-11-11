namespace ShoppingBasket.Abstractions;

public interface IShoppingBasketPricingService
{
    int CalculateTotalPence(IReadOnlyDictionary<string, int> quantities);
    decimal CalculateTotalPounds(IReadOnlyDictionary<string, int> quantities);
}
