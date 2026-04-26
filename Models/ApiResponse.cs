using System.Text.Json.Serialization;

namespace PhoneCompare.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class BrandListData
{
    [JsonPropertyName("brands")]
    public List<Brand> Brands { get; set; } = [];
}

public class PhoneListData
{
    [JsonPropertyName("phones")]
    public List<Phone> Phones { get; set; } = [];
}

public class PhoneDetailData
{
    [JsonPropertyName("phone_name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("phone_image")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("os")]
    public string Os { get; set; } = string.Empty;

    [JsonPropertyName("storage")]
    public string Storage { get; set; } = string.Empty;

    [JsonPropertyName("specifications")]
    public List<SpecGroup> Specifications { get; set; } = [];
}
