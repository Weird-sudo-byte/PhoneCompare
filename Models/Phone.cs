using System.Text.Json.Serialization;

namespace PhoneCompare.Models;

public class Phone
{
    [JsonPropertyName("phone_name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("phone_slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("phone_image")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("phone_id")]
    public int Id { get; set; }

    public string BrandName { get; set; } = string.Empty;

    public decimal PricePHP { get; set; }

    public string PriceDisplay => PricePHP > 0 ? $"₱{PricePHP:N0}" : "Price TBA";

    public bool IsFavorite { get; set; }

    public string FavoriteIcon => IsFavorite ? "♥" : "♡";
}
