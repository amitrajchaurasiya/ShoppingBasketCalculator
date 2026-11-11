using Microsoft.Extensions.DependencyInjection;
using ShoppingBasket.Abstractions;
using ShoppingBasket.DependencyInjection;

var services = new ServiceCollection();
services.AddShoppingBasket();

using var serviceProvider = services.BuildServiceProvider();
var pricingService = serviceProvider.GetRequiredService<IShoppingBasketPricingService>();

Console.WriteLine("Shopping Basket Price Calculator");
Console.WriteLine("--------------------------------");
Console.WriteLine("Products: Apple (35p), Banana (20p), Melon (50p), Lime (15p)");
Console.WriteLine("Offers: Melon - buy one get one free; Lime - three for the price of two.");
Console.WriteLine();
Console.Write("Enter products and quantities (e.g., Apple: 2, Banana: 3): ");

var input = Console.ReadLine() ?? string.Empty;

try
{
    var quantities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    foreach (var entry in input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
    {
        var parts = entry.Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) ||
            !int.TryParse(parts[1], out var quantity) || quantity <= 0)
        {
            throw new ArgumentException(
                $"Invalid entry '{entry}'. Use the format 'Product: quantity' with a positive quantity.");
        }

        var productName = parts[0];
        var existingQuantity = quantities.GetValueOrDefault(productName);
        if (quantity > int.MaxValue - existingQuantity)
        {
            throw new ArgumentException(
                $"The total quantity for '{productName}' exceeds the supported range.");
        }

        quantities[productName] = existingQuantity + quantity;
    }

    var totalPence = pricingService.CalculateTotalPence(quantities);
    Console.WriteLine($"Total: {totalPence}p (£{totalPence / 100m:F2})");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Unable to calculate basket: {ex.Message}");
    Environment.ExitCode = 1;
}
