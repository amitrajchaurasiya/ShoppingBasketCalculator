namespace ShoppingBasket.Models;

/// <summary>
/// Represents the configuration for a percentage discount offer, including the discount percentage.
/// </summary>
public sealed class PercentageDiscountConfiguration
{
    public PercentageDiscountConfiguration()
    {
    }

    public PercentageDiscountConfiguration(decimal discountPercentage)
    {
        DiscountPercentage = discountPercentage;
        Validate();
    }

    public decimal DiscountPercentage { get; set; }

    public void Validate()
    {
        if (DiscountPercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(
                nameof(DiscountPercentage),
                "Discount percentage must be between zero and one hundred.");
    }
}
