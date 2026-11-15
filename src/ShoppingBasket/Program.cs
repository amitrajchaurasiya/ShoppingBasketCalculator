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
Console.Write("Enter products separated by commas (e.g., Apple, Apple, Banana): ");

var input = Console.ReadLine() ?? string.Empty;

try
{
    var items = input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var totalPence = pricingService.CalculateTotalPence(items);
    Console.WriteLine($"Total: {totalPence}p (£{totalPence / 100m:F2})");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Unable to calculate basket: {ex.Message}");
    Environment.ExitCode = 1;
}
