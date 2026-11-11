using Microsoft.Extensions.DependencyInjection;
using ShoppingBasket.Abstractions;
using ShoppingBasket.DependencyInjection;
using ShoppingBasket.Models;
using ShoppingBasket.Offers;
using ShoppingBasket.Services;
using Xunit;

namespace ShoppingBasket.Tests;

public class ShoppingBasketPricingServiceTests
{
    private static IShoppingBasketPricingService CreateService()
    {
        var services = new ServiceCollection();
        services.AddShoppingBasket();
        return services.BuildServiceProvider()
            .GetRequiredService<IShoppingBasketPricingService>();
    }

    [Fact]
    public void EmptyBasket_ReturnsZero()
    {
        Assert.Equal(0, CreateService().CalculateTotalPence(Basket()));
    }

    [Theory]
    [InlineData("Apple", 35)]
    [InlineData("Banana", 20)]
    [InlineData("Melon", 50)]
    [InlineData("Lime", 15)]
    public void SingleItem_UsesUnitPrice(string item, int expected)
    {
        Assert.Equal(expected, CreateService().CalculateTotalPence(Basket((item, 1))));
    }

    [Theory]
    [InlineData(1, 50)]
    [InlineData(2, 50)]
    [InlineData(3, 100)]
    [InlineData(4, 100)]
    [InlineData(5, 150)]
    public void Melons_ApplyBuyOneGetOneFree(int quantity, int expected)
    {
        Assert.Equal(expected, CreateService().CalculateTotalPence(Basket(("Melon", quantity))));
    }

    [Theory]
    [InlineData(1, 15)]
    [InlineData(2, 30)]
    [InlineData(3, 30)]
    [InlineData(4, 45)]
    [InlineData(5, 60)]
    [InlineData(6, 60)]
    [InlineData(7, 75)]
    public void Limes_ApplyThreeForTwo(int quantity, int expected)
    {
        Assert.Equal(expected, CreateService().CalculateTotalPence(Basket(("Lime", quantity))));
    }

    [Fact]
    public void PercentageDiscount_AppliesToEachUnit()
    {
        var service = CreateOfferService(
            100,
            OfferRule.Create(
                OfferTypeIds.PercentageDiscount,
                new PercentageDiscountConfiguration(33m)));

        Assert.Equal(134, service.CalculateTotalPence(Basket(("Apple", 2))));
    }

    [Fact]
    public void PercentageDiscount_CombinesWithQuantityOffer()
    {
        var service = CreateOfferService(
            100,
            OfferRule.Create(
                OfferTypeIds.PercentageDiscount,
                new PercentageDiscountConfiguration(33m)),
            OfferRule.Create(
                OfferTypeIds.QuantityBundle,
                new QuantityBundleConfiguration(2, 1)));

        Assert.Equal(134, service.CalculateTotalPence(Basket(("Apple", 3))));
    }

    [Fact]
    public void PercentageDiscount_RoundsOnceAfterSummingBasket()
    {
        var service = CreateOfferService(
            1,
            OfferRule.Create(
                OfferTypeIds.PercentageDiscount,
                new PercentageDiscountConfiguration(33m)));

        Assert.Equal(1, service.CalculateTotalPence(Basket(("Apple", 1), ("Banana", 1))));
    }

    [Fact]
    public void DuplicateStrategyForOfferType_IsRejected()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ShoppingBasketPricingService(
                new OfferProductCatalog(100, []),
                [new QuantityOfferStrategy(), new QuantityOfferStrategy()]));
    }

    [Fact]
    public void MissingStrategyForConfiguredOffer_ThrowsInvalidOperationException()
    {
        var service = new ShoppingBasketPricingService(
            new OfferProductCatalog(
                100,
                [OfferRule.Create(
                    OfferTypeIds.PercentageDiscount,
                    new PercentageDiscountConfiguration(33m))]),
            []);

        Assert.Throws<InvalidOperationException>(() => service.CalculateTotalPence(Basket(("Apple", 1))));
    }

    [Fact]
    public void CustomOfferType_UsesRegisteredStrategyWithoutSchemaChanges()
    {
        var offer = OfferRule.Create("fixed-amount-discount", new { AmountPence = 10 });
        var service = new ShoppingBasketPricingService(
            new OfferProductCatalog(100, [offer]),
            [new FixedAmountDiscountStrategy()]);

        Assert.Equal(90, service.CalculateTotalPence(Basket(("Apple", 1))));
    }

    [Fact]
    public void InvalidConfigurationJson_ThrowsInvalidOperationException()
    {
        var service = new ShoppingBasketPricingService(
            new OfferProductCatalog(
                100,
                [new OfferRule(OfferTypeIds.PercentageDiscount, "{invalid json")]),
            [new PercentageDiscountStrategy()]);

        Assert.Throws<InvalidOperationException>(() => service.CalculateTotalPence(Basket(("Apple", 1))));
    }

    [Fact]
    public void UnsupportedConfigurationVersion_ThrowsInvalidOperationException()
    {
        var service = new ShoppingBasketPricingService(
            new OfferProductCatalog(
                100,
                [OfferRule.Create(
                    OfferTypeIds.PercentageDiscount,
                    new PercentageDiscountConfiguration(33m),
                    configurationVersion: 2)]),
            [new PercentageDiscountStrategy()]);

        Assert.Throws<InvalidOperationException>(() => service.CalculateTotalPence(Basket(("Apple", 1))));
    }

    [Fact]
    public void MixedBasket_CalculatesExpectedTotal()
    {
        var basket = Basket(("Apple", 2), ("Banana", 1), ("Melon", 2), ("Lime", 3));
        // 70p + 20p + 50p + 30p = 170p.
        Assert.Equal(170, CreateService().CalculateTotalPence(basket));
    }

    [Fact]
    public void ProductNames_AreCaseInsensitiveAndTrimmed()
    {
        Assert.Equal(55, CreateService().CalculateTotalPence(Basket((" apple ", 1), ("BANANA", 1))));
    }

    [Fact]
    public void DuplicateProductKeys_AreCombinedCaseInsensitively()
    {
        var basket = new Dictionary<string, int>
        {
            ["Apple"] = 1,
            ["apple"] = 1
        };

        Assert.Equal(70, CreateService().CalculateTotalPence(basket));
    }

    [Fact]
    public void UnknownProduct_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateService().CalculateTotalPence(Basket(("Apple", 1), ("Pear", 1))));
    }

    [Fact]
    public void BlankProduct_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateService().CalculateTotalPence(Basket(("Apple", 1), (" ", 1))));
    }

    [Fact]
    public void NonPositiveQuantity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateService().CalculateTotalPence(Basket(("Apple", 0))));
    }

    [Fact]
    public void NullBasket_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CreateService().CalculateTotalPence(null!));
    }

    private static IReadOnlyDictionary<string, int> Basket(params (string ProductName, int Quantity)[] entries)
    {
        var quantities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (productName, quantity) in entries)
            quantities[productName] = checked(quantities.GetValueOrDefault(productName) + quantity);

        return quantities;
    }

    private static IShoppingBasketPricingService CreateOfferService(int unitPricePence, params OfferRule[] offers) =>
        new ShoppingBasketPricingService(
            new OfferProductCatalog(unitPricePence, offers),
            [new PercentageDiscountStrategy(), new QuantityOfferStrategy()]);

    private sealed class OfferProductCatalog(int unitPricePence, IReadOnlyList<OfferRule> offers) : IProductCatalog
    {
        public Product GetByName(string productName) => new(productName, unitPricePence, offers);
    }

    private sealed class FixedAmountDiscountStrategy : IPricingOfferStrategy
    {
        public string OfferType => "fixed-amount-discount";

        public int Priority => 150;

        public decimal Apply(
            OfferRule offer,
            int quantity,
            int unitPricePence,
            decimal currentLineTotalPence) => currentLineTotalPence - 10m;
    }
}
