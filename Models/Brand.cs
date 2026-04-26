using System.Text.Json.Serialization;

namespace PhoneCompare.Models;

public class Brand
{
    [JsonPropertyName("brand_name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("brand_slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("brand_id")]
    public int Id { get; set; }
}
