using System.Net.Http.Json;
using System.Text.Json;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

public class PhoneApiService : IPhoneApiService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://phone-specs-api.azharimm.dev/v2";
    private bool _useMockData = false;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // Known popular brands to display in the dropdown
    private static readonly HashSet<string> KnownBrandSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "apple", "samsung", "xiaomi", "realme", "huawei", "oppo", "vivo",
        "oneplus", "google", "motorola", "nokia", "sony", "asus",
        "infinix", "tecno", "itel", "honor", "nothing", "poco"
    };

    public PhoneApiService(HttpClient http) => _http = http;

    public async Task<List<Brand>> GetBrandsAsync()
    {
        // Return hardcoded known brands for reliability
        var knownBrands = new List<Brand>
        {
            new() { Name = "Apple", Slug = "apple" },
            new() { Name = "Samsung", Slug = "samsung" },
            new() { Name = "Xiaomi", Slug = "xiaomi" },
            new() { Name = "Realme", Slug = "realme" },
            new() { Name = "Oppo", Slug = "oppo" },
            new() { Name = "Vivo", Slug = "vivo" },
            new() { Name = "Huawei", Slug = "huawei" },
            new() { Name = "OnePlus", Slug = "oneplus" },
            new() { Name = "Google", Slug = "google" },
            new() { Name = "Motorola", Slug = "motorola" },
            new() { Name = "Nokia", Slug = "nokia" },
            new() { Name = "Sony", Slug = "sony" },
            new() { Name = "Asus", Slug = "asus" },
            new() { Name = "Infinix", Slug = "infinix" },
            new() { Name = "Tecno", Slug = "tecno" },
            new() { Name = "Itel", Slug = "itel" },
            new() { Name = "Honor", Slug = "honor" },
            new() { Name = "Nothing", Slug = "nothing" },
            new() { Name = "Poco", Slug = "poco" }
        };
        
        System.Diagnostics.Debug.WriteLine($"[PhoneApi] Returning {knownBrands.Count} known brands");
        return await Task.FromResult(knownBrands.OrderBy(b => b.Name).ToList());
    }

    public async Task<List<Phone>> GetPhonesByBrandAsync(string brandSlug)
    {
        System.Diagnostics.Debug.WriteLine($"[PhoneApi] GetPhonesByBrand called with slug: '{brandSlug}'");
        
        // Always try API first for brand phones, regardless of _useMockData
        try
        {
            var url = $"{BaseUrl}/brands/{brandSlug}";
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Fetching: {url}");
            
            var r = await _http.GetAsync(url);
            var json = await r.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Response status: {r.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Brand '{brandSlug}' response: {json.Substring(0, Math.Min(800, json.Length))}");
            
            var result = JsonSerializer.Deserialize<ApiResponse<PhoneListData>>(json, JsonOpts);
            if (result?.Data?.Phones?.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[PhoneApi] Parsed {result.Data.Phones.Count} phones for '{brandSlug}'");
                return result.Data.Phones;
            }
            
            var direct = JsonSerializer.Deserialize<ApiResponse<List<Phone>>>(json, JsonOpts);
            var count = direct?.Data?.Count ?? 0;
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Direct parse: {count} phones for '{brandSlug}'");
            return direct?.Data ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] GetByBrand error for '{brandSlug}': {ex.Message}");
            return [];
        }
    }

    public async Task<PhoneDetail?> GetPhoneDetailAsync(string phoneSlug)
    {
        if (_useMockData) return await Task.FromResult(GetMockPhoneDetail(phoneSlug));
        
        try
        {
            var r = await _http.GetAsync($"{BaseUrl}/{phoneSlug}");
            var json = await r.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Detail response: {json.Substring(0, Math.Min(500, json.Length))}");
            var result = JsonSerializer.Deserialize<ApiResponse<PhoneDetailData>>(json, JsonOpts);
            if (result?.Data == null) return null;

            return new PhoneDetail
            {
                Name           = result.Data.Name,
                ImageUrl       = result.Data.ImageUrl,
                Os             = result.Data.Os,
                Storage        = result.Data.Storage,
                Specifications = result.Data.Specifications,
                Slug           = phoneSlug
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] GetDetail error: {ex.Message}");
            _useMockData = true;
            return GetMockPhoneDetail(phoneSlug);
        }
    }

    public async Task<List<Phone>> SearchPhonesAsync(string query)
    {
        if (_useMockData) return await Task.FromResult(GetMockPhones().Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList());
        
        try
        {
            var encoded = Uri.EscapeDataString(query);
            var r = await _http.GetAsync($"{BaseUrl}/search?query={encoded}");
            var json = await r.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Search response: {json.Substring(0, Math.Min(500, json.Length))}");
            
            var result = JsonSerializer.Deserialize<ApiResponse<PhoneListData>>(json, JsonOpts);
            if (result?.Data?.Phones?.Count > 0)
                return result.Data.Phones;
            
            var direct = JsonSerializer.Deserialize<ApiResponse<List<Phone>>>(json, JsonOpts);
            return direct?.Data ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Search error: {ex.Message}");
            _useMockData = true;
            return GetMockPhones().Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }

    public async Task<List<Phone>> GetLatestPhonesAsync()
    {
        if (_useMockData) return await Task.FromResult(GetMockPhones());
        
        try
        {
            var r = await _http.GetAsync($"{BaseUrl}/latest");
            var json = await r.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Latest response: {json.Substring(0, Math.Min(500, json.Length))}");
            
            var result = JsonSerializer.Deserialize<ApiResponse<PhoneListData>>(json, JsonOpts);
            if (result?.Data?.Phones?.Count > 0)
                return result.Data.Phones;
            
            var direct = JsonSerializer.Deserialize<ApiResponse<List<Phone>>>(json, JsonOpts);
            return direct?.Data ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] GetLatest error: {ex.Message} - switching to mock data");
            _useMockData = true;
            return GetMockPhones();
        }
    }

    public async Task<List<Phone>> GetTopPhonesAsync()
    {
        if (_useMockData) return await Task.FromResult(GetMockPhones().Take(5).ToList());
        
        try
        {
            var r = await _http.GetAsync($"{BaseUrl}/top-by-interest");
            var json = await r.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] Top response: {json.Substring(0, Math.Min(500, json.Length))}");
            
            var result = JsonSerializer.Deserialize<ApiResponse<PhoneListData>>(json, JsonOpts);
            if (result?.Data?.Phones?.Count > 0)
                return result.Data.Phones;
            
            var direct = JsonSerializer.Deserialize<ApiResponse<List<Phone>>>(json, JsonOpts);
            return direct?.Data ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PhoneApi] GetTop error: {ex.Message}");
            _useMockData = true;
            return GetMockPhones().Take(5).ToList();
        }
    }

    #region Mock Data

    private static List<Brand>  GetMockBrands()             => MockPhoneData.GetBrands();
    private static List<Phone>  GetMockPhones()             => MockPhoneData.GetPhones();
    private static PhoneDetail  GetMockPhoneDetail(string s) => MockPhoneData.BuildDetail(s);

    #endregion
}
