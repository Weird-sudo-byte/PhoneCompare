using PhoneCompare.Models;

namespace PhoneCompare.Services;

public class HybridPhoneService : IPhoneApiService
{
    private readonly FirestorePhoneService _firestore;
    private readonly GsmArenaScraperService _scraper;
    private readonly PhoneSyncService _sync;
    private readonly PhoneDataCache _cache;

    private static readonly TimeSpan ListTtl   = TimeSpan.FromDays(7);
    private static readonly TimeSpan DetailTtl  = TimeSpan.FromDays(30);

    public HybridPhoneService(
        FirestorePhoneService firestore,
        GsmArenaScraperService scraper,
        PhoneSyncService sync,
        PhoneDataCache cache)
    {
        _firestore = firestore;
        _scraper = scraper;
        _sync = sync;
        _cache = cache;
    }

    // ── Flow: Cache → Firestore → Scraper (+ auto-sync) → Mock ─────────

    public async Task<List<Brand>> GetBrandsAsync()
    {
        const string key = "brands";
        var cached = _cache.TryGet<List<Brand>>(key, ListTtl);
        if (cached is { Count: > 0 }) return cached;

        // Try Firestore
        try
        {
            var fb = await _firestore.GetBrandsAsync();
            if (fb.Count > 0) { _cache.Set(key, fb); return fb; }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Hybrid] Firestore brands failed: {ex.Message}");
        }

        // Try scraper
        try
        {
            var brands = await _scraper.ScrapeBrandsAsync();
            if (brands.Count > 0)
            {
                _cache.Set(key, brands);
                _ = Task.Run(() => _sync.SyncBrandsAsync(brands)); // auto-sync
                return brands;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Hybrid] ScrapeBrands failed: {ex.Message}");
        }

        return MockPhoneData.GetBrands();
    }

    public async Task<List<Phone>> GetPhonesByBrandAsync(string brandSlug)
    {
        var key = $"phones_{brandSlug}";
        var cached = _cache.TryGet<List<Phone>>(key, ListTtl);
        if (cached is { Count: > 0 }) return cached;

        // Try Firestore
        try
        {
            var fb = await _firestore.GetPhonesByBrandAsync(brandSlug);
            if (fb.Count > 0) { _cache.Set(key, fb); return fb; }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Hybrid] Firestore phones failed: {ex.Message}");
        }

        // Try scraper
        try
        {
            var phones = await _scraper.ScrapePhonesByBrandAsync(brandSlug);
            if (phones.Count > 0)
            {
                _cache.Set(key, phones);
                _ = Task.Run(() => _sync.SyncPhonesAsync(phones)); // auto-sync
                return phones;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Hybrid] ScrapeByBrand failed: {ex.Message}");
        }

        return MockPhoneData.GetPhones()
            .Where(p => p.BrandName.Equals(brandSlug, StringComparison.OrdinalIgnoreCase)
                     || p.Slug.Contains(brandSlug))
            .ToList();
    }

    public async Task<PhoneDetail?> GetPhoneDetailAsync(string phoneSlug)
    {
        var key = $"detail_{phoneSlug}";
        var cached = _cache.TryGet<PhoneDetail>(key, DetailTtl);
        if (cached != null) return cached;

        // Try Firestore
        try
        {
            var fb = await _firestore.GetPhoneDetailAsync(phoneSlug);
            if (fb != null) { _cache.Set(key, fb); return fb; }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Hybrid] Firestore detail failed: {ex.Message}");
        }

        // Try scraper
        try
        {
            var detail = await _scraper.ScrapePhoneDetailAsync(phoneSlug);
            if (detail != null)
            {
                _cache.Set(key, detail);
                _ = Task.Run(() => _sync.SyncPhoneDetailAsync(detail)); // auto-sync
                return detail;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Hybrid] ScrapeDetail failed: {ex.Message}");
        }

        return MockPhoneData.BuildDetail(phoneSlug);
    }

    public async Task<List<Phone>> SearchPhonesAsync(string query)
    {
        // Try Firestore first
        try
        {
            var fb = await _firestore.SearchPhonesAsync(query);
            if (fb.Count > 0) return fb;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Hybrid] Firestore search failed: {ex.Message}");
        }

        // Try scraper
        try
        {
            var results = await _scraper.ScrapeSearchAsync(query);
            if (results.Count > 0) return results;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Hybrid] ScrapeSearch failed: {ex.Message}");
        }

        return MockPhoneData.GetPhones()
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<List<Phone>> GetLatestPhonesAsync()
    {
        // Try Firestore for all phones, return newest
        try
        {
            var all = await _firestore.GetAllPhonesAsync();
            if (all.Count > 0) return all.Take(20).ToList();
        }
        catch { }

        return await Task.FromResult(MockPhoneData.GetPhones());
    }

    public async Task<List<Phone>> GetTopPhonesAsync()
    {
        // Try Firestore
        try
        {
            var all = await _firestore.GetAllPhonesAsync();
            if (all.Count > 0) return all.Take(10).ToList();
        }
        catch { }

        return await Task.FromResult(MockPhoneData.GetPhones().Take(10).ToList());
    }
}
