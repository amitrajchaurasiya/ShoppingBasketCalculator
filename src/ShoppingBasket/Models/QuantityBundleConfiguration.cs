namespace ShoppingBasket.Models;

/// <summary>
/// Represents the configuration for a quantity bundle offer, including the buy quantity and pay-for quantity.
/// </summary>
public sealed class QuantityBundleConfiguration
{
    public QuantityBundleConfiguration()
    {
    }

    public QuantityBundleConfiguration(int buyQuantity, int payForQuantity)
    {
        BuyQuantity = buyQuantity;
        PayForQuantity = payForQuantity;
        Validate();
    }

    public int BuyQuantity { get; set; }

    public int PayForQuantity { get; set; }

    public void Validate()
    {
        if (BuyQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(BuyQuantity), "Buy quantity must be positive.");

        if (PayForQuantity < 0 || PayForQuantity > BuyQuantity)
            throw new ArgumentOutOfRangeException(
                nameof(PayForQuantity),
                "Pay-for quantity must be between zero and the buy quantity.");
    }
}
