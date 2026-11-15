using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ShoppingBasket.Models;

/// <summary>
/// Represents a pricing offer rule for a product, including the offer type, configuration, and version.
/// </summary>
public sealed class OfferRule
{
    private static readonly JsonSerializerOptions ConfigurationJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OfferRule()
    {
    }

    public OfferRule(string offerType, string configurationJson, int configurationVersion = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(offerType);
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationJson);
        if (configurationVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(configurationVersion));

        OfferType = offerType;
        ConfigurationJson = configurationJson;
        ConfigurationVersion = configurationVersion;
    }

    public static OfferRule Create<TConfiguration>(
        string offerType,
        TConfiguration configuration,
        int configurationVersion = 1)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return new OfferRule(offerType, JsonSerializer.Serialize(configuration), configurationVersion);
    }

    public TConfiguration DeserializeConfiguration<TConfiguration>(int supportedVersion = 1)
        where TConfiguration : class
    {
        if (ConfigurationVersion != supportedVersion)
            throw new InvalidOperationException(
                $"Offer '{OfferType}' uses unsupported configuration version {ConfigurationVersion}.");

        try
        {
            return JsonSerializer.Deserialize<TConfiguration>(ConfigurationJson, ConfigurationJsonOptions)
                ?? throw new InvalidOperationException($"Offer '{OfferType}' has an empty configuration.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Offer '{OfferType}' has invalid JSON configuration.", exception);
        }
    }

    public int ProductOfferId { get; set; }

    public int ProductId { get; set; }

    [JsonIgnore]
    public Product? Product { get; set; }

    public string OfferType { get; set; } = string.Empty;

    public string ConfigurationJson { get; set; } = "{}";

    public int ConfigurationVersion { get; set; } = 1;
}
