using System.Text.Json.Serialization;

namespace PhoneCompare.Models;

public class PhoneDetail
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

    public string Slug { get; set; } = string.Empty;

    public string Display    => GetSpec("Display",  "Size")    ?? GetSpec("Display",  "Type") ?? "N/A";
    public string Chipset    => GetSpec("Platform", "Chipset") ?? "N/A";
    public string Ram        => GetSpec("Memory",   "Internal") ?? "N/A";
    public string MainCamera => GetSpec("Main Camera", "Single")
                             ?? GetSpec("Main Camera", "Dual")
                             ?? GetSpec("Main Camera", "Triple") ?? "N/A";
    public string Battery    => GetSpec("Battery",  "Type") ?? "N/A";
    public string Price      => GetSpec("Misc",     "Price") ?? "N/A";

    private string? GetSpec(string group, string key)
    {
        var g = Specifications.FirstOrDefault(s =>
            s.Title.Contains(group, StringComparison.OrdinalIgnoreCase));
        if (g == null) return null;

        if (string.IsNullOrEmpty(key))
            return g.Specs.FirstOrDefault()?.Value;

        return g.Specs.FirstOrDefault(s =>
            s.Key.Contains(key, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}

public class SpecGroup
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("specs")]
    public List<SpecItem> Specs { get; set; } = [];
}

public class SpecItem
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("val")]
    public List<string> Val { get; set; } = [];

    public string Value => Val.Count > 0 ? string.Join(", ", Val) : "N/A";
}
