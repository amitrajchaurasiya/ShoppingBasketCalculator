using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingBasket.Models;

public sealed class Product
{
    public Product()
    {
    }

    public Product(string name, int unitPricePence, IEnumerable<OfferRule>? offers = null)
    {
        Name = name;
        UnitPricePence = unitPricePence;
        Offers = offers?.ToList() ?? new List<OfferRule>();
    }

    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int UnitPricePence { get; set; }

    public ICollection<OfferRule> Offers { get; set; } = new List<OfferRule>();
}
