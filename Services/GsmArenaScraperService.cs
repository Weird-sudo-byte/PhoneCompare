using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

public class GsmArenaScraperService
{
    private const string BaseUrl = "https://www.gsmarena.com";
    private static readonly Random Rng = new();
    private static DateTime _lastRequest = DateTime.MinValue;
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly HttpClient _http;

    public GsmArenaScraperService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");
        _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        _http.Timeout = TimeSpan.FromSeconds(15);
    }

    // ── Rate limiter ─────────────────────────────────────────────────────────
    private async Task RateLimit()
    {
        await Gate.WaitAsync();
        try
        {
            var elapsed = DateTime.UtcNow - _lastRequest;
            var delay = TimeSpan.FromSeconds(3) + TimeSpan.FromMilliseconds(Rng.Next(500, 1500));
            if (elapsed < delay)
                await Task.Delay(delay - elapsed);
            _lastRequest = DateTime.UtcNow;
        }
        finally { Gate.Release(); }
    }

    private async Task<HtmlDocument> FetchPage(string url)
    {
        await RateLimit();
        System.Diagnostics.Debug.WriteLine($"[Scraper] Fetching: {url}");
        var html = await _http.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    // ── Scrape brands ────────────────────────────────────────────────────────
    public async Task<List<Brand>> ScrapeBrandsAsync()
    {
        var doc = await FetchPage($"{BaseUrl}/makers.php3");
        var brands = new List<Brand>();
        var nodes = doc.DocumentNode.SelectNodes("//div[@class='st-text']/table//a");
        if (nodes == null) return brands;

        int id = 1;
        foreach (var a in nodes)
        {
            var href = a.GetAttributeValue("href", "");
            var name = a.SelectSingleNode(".//br")?.PreviousSibling?.InnerText?.Trim();
            if (string.IsNullOrEmpty(name))
                name = HtmlEntity.DeEntitize(a.InnerText).Trim();

            // clean name: "Samsung218 devices" -> "Samsung"
            name = Regex.Replace(name, @"\d+\s*devices?", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(href)) continue;

            var slug = href.Replace("-phones-f-1.php", "")
                           .Replace("-phones-1.php", "")
                           .Replace(".php", "")
                           .Trim('/');

            brands.Add(new Brand { Id = id++, Name = name, Slug = slug });
        }

        return brands;
    }

    // ── Scrape phones by brand ───────────────────────────────────────────────
    public async Task<List<Phone>> ScrapePhonesByBrandAsync(string brandSlug, int maxPages = 2)
    {
        var phones = new List<Phone>();
        int id = 1;

        for (int page = 1; page <= maxPages; page++)
        {
            var url = page == 1
                ? $"{BaseUrl}/{brandSlug}-phones-f-1.php"
                : $"{BaseUrl}/{brandSlug}-phones-f-{page}.php";

            HtmlDocument doc;
            try { doc = await FetchPage(url); }
            catch { break; }

            var items = doc.DocumentNode.SelectNodes("//div[@class='makers']//li");
            if (items == null || items.Count == 0) break;

            foreach (var li in items)
            {
                var a = li.SelectSingleNode(".//a");
                var img = li.SelectSingleNode(".//img");
                var nameNode = li.SelectSingleNode(".//span");

                var href = a?.GetAttributeValue("href", "") ?? "";
                var phoneName = nameNode != null
                    ? HtmlEntity.DeEntitize(nameNode.InnerText).Trim()
                    : "";
                var imageUrl = img?.GetAttributeValue("src", "") ?? "";

                if (string.IsNullOrEmpty(phoneName) || string.IsNullOrEmpty(href)) continue;

                var slug = href.Replace(".php", "").Trim('/');
                var brand = char.ToUpper(brandSlug[0]) + brandSlug[1..];

                phones.Add(new Phone
                {
                    Id = id++,
                    Name = phoneName,
                    Slug = slug,
                    ImageUrl = imageUrl,
                    BrandName = brand
                });
            }

            // Check if there's a next page
            var nextLink = doc.DocumentNode.SelectSingleNode("//a[@class='pages-next']");
            if (nextLink == null) break;
        }

        return phones;
    }

    // ── Scrape phone detail ──────────────────────────────────────────────────
    public async Task<PhoneDetail?> ScrapePhoneDetailAsync(string slug)
    {
        HtmlDocument doc;
        try { doc = await FetchPage($"{BaseUrl}/{slug}.php"); }
        catch { return null; }

        var name = doc.DocumentNode.SelectSingleNode("//h1[@data-spec='modelname']")
                    ?? doc.DocumentNode.SelectSingleNode("//h1");
        var img = doc.DocumentNode.SelectSingleNode("//div[@class='specs-photo-main']//img");

        var phoneName = name != null ? HtmlEntity.DeEntitize(name.InnerText).Trim() : slug;
        var imageUrl = img?.GetAttributeValue("src", "") ?? "";

        var specGroups = new List<SpecGroup>();
        var tables = doc.DocumentNode.SelectNodes("//table");

        if (tables != null)
        {
            foreach (var table in tables)
            {
                var thNode = table.SelectSingleNode(".//th");
                if (thNode == null) continue;

                var groupTitle = HtmlEntity.DeEntitize(thNode.InnerText).Trim();
                if (string.IsNullOrEmpty(groupTitle)) continue;

                var specs = new List<SpecItem>();
                var rows = table.SelectNodes(".//tr");
                if (rows == null) continue;

                foreach (var row in rows)
                {
                    var keyNode = row.SelectSingleNode(".//td[@class='ttl']//a")
                                ?? row.SelectSingleNode(".//td[@class='ttl']");
                    var valNode = row.SelectSingleNode(".//td[@class='nfo']");

                    if (keyNode == null || valNode == null) continue;

                    var key = HtmlEntity.DeEntitize(keyNode.InnerText).Trim();
                    var val = HtmlEntity.DeEntitize(valNode.InnerHtml)
                        .Replace("<br>", "\n")
                        .Replace("<br/>", "\n")
                        .Replace("<br />", "\n");
                    // Strip remaining HTML tags
                    val = Regex.Replace(val, "<[^>]+>", "").Trim();

                    if (!string.IsNullOrEmpty(key))
                    {
                        var values = val.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(v => v.Trim())
                                        .Where(v => !string.IsNullOrEmpty(v))
                                        .ToList();
                        if (values.Count == 0) values.Add(val.Trim());

                        specs.Add(new SpecItem { Key = key, Val = values });
                    }
                }

                if (specs.Count > 0)
                    specGroups.Add(new SpecGroup { Title = groupTitle, Specs = specs });
            }
        }

        return new PhoneDetail
        {
            Name = phoneName,
            ImageUrl = imageUrl,
            Slug = slug,
            Specifications = specGroups
        };
    }

    // ── Search (scrape search page) ──────────────────────────────────────────
    public async Task<List<Phone>> ScrapeSearchAsync(string query)
    {
        var encoded = Uri.EscapeDataString(query);
        var doc = await FetchPage($"{BaseUrl}/results.php3?sQuickSearch=yes&sName={encoded}");

        var phones = new List<Phone>();
        var items = doc.DocumentNode.SelectNodes("//div[@class='makers']//li");
        if (items == null) return phones;

        int id = 1;
        foreach (var li in items)
        {
            var a = li.SelectSingleNode(".//a");
            var img = li.SelectSingleNode(".//img");
            var nameNode = li.SelectSingleNode(".//span");

            var href = a?.GetAttributeValue("href", "") ?? "";
            var phoneName = nameNode != null
                ? HtmlEntity.DeEntitize(nameNode.InnerText).Trim()
                : "";
            var imageUrl = img?.GetAttributeValue("src", "") ?? "";

            if (string.IsNullOrEmpty(phoneName)) continue;

            var slug = href.Replace(".php", "").Trim('/');

            phones.Add(new Phone
            {
                Id = id++,
                Name = phoneName,
                Slug = slug,
                ImageUrl = imageUrl,
                BrandName = phoneName.Split(' ').FirstOrDefault() ?? ""
            });
        }

        return phones;
    }
}
