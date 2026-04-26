using System.Text.Json;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

public class RecentActivityService
{
    private const string RecentSearchesKey = "recent_searches";
    private const string RecentlyViewedKey = "recently_viewed";
    private const int MaxItems = 10;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public List<string> GetRecentSearches()
    {
        try
        {
            var json = Preferences.Get(RecentSearchesKey, "[]");
            return JsonSerializer.Deserialize<List<string>>(json, JsonOpts) ?? [];
        }
        catch { return []; }
    }

    public void AddRecentSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;
        
        var searches = GetRecentSearches();
        searches.Remove(query); // Remove if exists to move to top
        searches.Insert(0, query);
        
        if (searches.Count > MaxItems)
            searches = searches.Take(MaxItems).ToList();
        
        Preferences.Set(RecentSearchesKey, JsonSerializer.Serialize(searches));
    }

    public void ClearRecentSearches()
    {
        Preferences.Remove(RecentSearchesKey);
    }

    public List<RecentPhone> GetRecentlyViewed()
    {
        try
        {
            var json = Preferences.Get(RecentlyViewedKey, "[]");
            return JsonSerializer.Deserialize<List<RecentPhone>>(json, JsonOpts) ?? [];
        }
        catch { return []; }
    }

    public void AddRecentlyViewed(string slug, string name, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(slug)) return;
        
        var phones = GetRecentlyViewed();
        phones.RemoveAll(p => p.Slug == slug); // Remove if exists
        phones.Insert(0, new RecentPhone
        {
            Slug = slug,
            Name = name,
            ImageUrl = imageUrl,
            ViewedAt = DateTime.UtcNow
        });
        
        if (phones.Count > MaxItems)
            phones = phones.Take(MaxItems).ToList();
        
        Preferences.Set(RecentlyViewedKey, JsonSerializer.Serialize(phones));
    }

    public void ClearRecentlyViewed()
    {
        Preferences.Remove(RecentlyViewedKey);
    }
}

public class RecentPhone
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime ViewedAt { get; set; }
}
