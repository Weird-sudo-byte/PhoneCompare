using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PhoneCompare.Config;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

/// <summary>
/// Firestore REST API service for reading/writing phone data collections:
/// brands, phones, phone_details.
/// </summary>
public class FirestorePhoneService
{
    private readonly HttpClient _http;
    private readonly IAuthService _auth;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private static string DbBase =>
        $"{FirebaseConfig.FirestoreBaseUrl}/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents";

    public FirestorePhoneService(HttpClient http, IAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    // ── Token helper ────────────────────────────────────────────────────────
    private string? GetToken() => _auth.GetCurrentUser()?.IdToken;

    private HttpRequestMessage AuthRequest(HttpMethod method, string url, HttpContent? content = null)
    {
        var req = new HttpRequestMessage(method, url) { Content = content };
        var token = GetToken();
        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  BRANDS
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<List<Brand>> GetBrandsAsync()
    {
        try
        {
            var url = $"{DbBase}/brands?pageSize=200";
            var req = AuthRequest(HttpMethod.Get, url);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return [];

            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("documents", out var docs)) return [];

            var brands = new List<Brand>();
            foreach (var d in docs.EnumerateArray())
            {
                var b = ParseBrand(d);
                if (b != null) brands.Add(b);
            }
            return brands;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firestore] GetBrands error: {ex.Message}");
            return [];
        }
    }

    public async Task<bool> SaveBrandAsync(Brand brand)
    {
        try
        {
            var url = $"{DbBase}/brands?documentId={Uri.EscapeDataString(brand.Slug)}";
            var body = BuildBrandDocument(brand);
            var content = new StringContent(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");
            var req = AuthRequest(HttpMethod.Post, url, content);
            // Use PATCH to upsert
            var patchUrl = $"{DbBase}/brands/{Uri.EscapeDataString(brand.Slug)}";
            var patchReq = AuthRequest(HttpMethod.Patch, patchUrl, content);
            var resp = await _http.SendAsync(patchReq);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firestore] SaveBrand error: {ex.Message}");
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PHONES
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<List<Phone>> GetPhonesByBrandAsync(string brandSlug)
    {
        try
        {
            // Use structured query to filter by brandSlug
            var queryUrl = $"{DbBase}:runQuery";
            var queryBody = new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "phones" } },
                    where = new
                    {
                        fieldFilter = new
                        {
                            field = new { fieldPath = "brandSlug" },
                            op = "EQUAL",
                            value = new { stringValue = brandSlug }
                        }
                    },
                    limit = 200
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(queryBody, JsonOpts), Encoding.UTF8, "application/json");
            var req = AuthRequest(HttpMethod.Post, queryUrl, content);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return [];

            var json = await resp.Content.ReadAsStringAsync();
            var arr = JsonSerializer.Deserialize<JsonElement[]>(json, JsonOpts);
            if (arr == null) return [];

            var phones = new List<Phone>();
            foreach (var item in arr)
            {
                if (!item.TryGetProperty("document", out var doc)) continue;
                var p = ParsePhone(doc);
                if (p != null) phones.Add(p);
            }
            return phones;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firestore] GetPhonesByBrand error: {ex.Message}");
            return [];
        }
    }

    public async Task<List<Phone>> GetAllPhonesAsync()
    {
        try
        {
            var url = $"{DbBase}/phones?pageSize=500";
            var req = AuthRequest(HttpMethod.Get, url);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return [];

            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("documents", out var docs)) return [];

            var phones = new List<Phone>();
            foreach (var d in docs.EnumerateArray())
            {
                var p = ParsePhone(d);
                if (p != null) phones.Add(p);
            }
            return phones;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firestore] GetAllPhones error: {ex.Message}");
            return [];
        }
    }

    public async Task<List<Phone>> SearchPhonesAsync(string query)
    {
        // Firestore doesn't support full-text search natively
        // Get all phones and filter client-side
        var all = await GetAllPhonesAsync();
        if (all.Count == 0) return [];
        return all.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<bool> SavePhoneAsync(Phone phone)
    {
        try
        {
            var docId = Uri.EscapeDataString(phone.Slug);
            var url = $"{DbBase}/phones/{docId}";
            var body = BuildPhoneDocument(phone);
            var content = new StringContent(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");
            var req = AuthRequest(HttpMethod.Patch, url, content);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firestore] SavePhone error: {ex.Message}");
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PHONE DETAILS
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<PhoneDetail?> GetPhoneDetailAsync(string phoneSlug)
    {
        try
        {
            var docId = Uri.EscapeDataString(phoneSlug);
            var url = $"{DbBase}/phone_details/{docId}";
            var req = AuthRequest(HttpMethod.Get, url);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return ParsePhoneDetail(doc.RootElement);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firestore] GetPhoneDetail error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SavePhoneDetailAsync(PhoneDetail detail, string source = "mock")
    {
        try
        {
            var docId = Uri.EscapeDataString(detail.Slug);
            var url = $"{DbBase}/phone_details/{docId}";
            var body = BuildPhoneDetailDocument(detail, source);
            var content = new StringContent(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");
            var req = AuthRequest(HttpMethod.Patch, url, content);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firestore] SavePhoneDetail error: {ex.Message}");
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  STATS
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<(int Brands, int Phones, int Details)> GetStatsAsync()
    {
        int brands = 0, phones = 0, details = 0;
        try
        {
            var b = await GetBrandsAsync();
            brands = b.Count;
        }
        catch { }
        try
        {
            var p = await GetAllPhonesAsync();
            phones = p.Count;
        }
        catch { }
        // Details count: just check if collection exists
        try
        {
            var url = $"{DbBase}/phone_details?pageSize=500&mask.fieldPaths=slug";
            var req = AuthRequest(HttpMethod.Get, url);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("documents", out var docs))
                    details = docs.GetArrayLength();
            }
        }
        catch { }
        return (brands, phones, details);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DOCUMENT BUILDERS
    // ═══════════════════════════════════════════════════════════════════════

    private static object BuildBrandDocument(Brand b) => new
    {
        fields = new Dictionary<string, object>
        {
            ["id"]   = new { integerValue = b.Id.ToString() },
            ["name"] = new { stringValue  = b.Name },
            ["slug"] = new { stringValue  = b.Slug },
            ["lastUpdated"] = new { timestampValue = DateTime.UtcNow.ToString("O") }
        }
    };

    private static object BuildPhoneDocument(Phone p) => new
    {
        fields = new Dictionary<string, object>
        {
            ["id"]        = new { integerValue = p.Id.ToString() },
            ["name"]      = new { stringValue  = p.Name },
            ["slug"]      = new { stringValue  = p.Slug },
            ["brandName"] = new { stringValue  = p.BrandName },
            ["brandSlug"] = new { stringValue  = p.BrandName.ToLowerInvariant() },
            ["imageUrl"]  = new { stringValue  = p.ImageUrl },
            ["lastUpdated"] = new { timestampValue = DateTime.UtcNow.ToString("O") }
        }
    };

    private static object BuildPhoneDetailDocument(PhoneDetail d, string source) => new
    {
        fields = new Dictionary<string, object>
        {
            ["slug"]     = new { stringValue = d.Slug },
            ["name"]     = new { stringValue = d.Name },
            ["imageUrl"] = new { stringValue = d.ImageUrl },
            ["os"]       = new { stringValue = d.Os ?? "" },
            ["storage"]  = new { stringValue = d.Storage ?? "" },
            ["source"]   = new { stringValue = source },
            ["lastUpdated"] = new { timestampValue = DateTime.UtcNow.ToString("O") },
            ["specifications"] = BuildSpecsArray(d.Specifications)
        }
    };

    private static object BuildSpecsArray(List<SpecGroup> specs) => new
    {
        arrayValue = new
        {
            values = specs.Select(g => new
            {
                mapValue = new
                {
                    fields = new Dictionary<string, object>
                    {
                        ["title"] = new { stringValue = g.Title },
                        ["specs"] = new
                        {
                            arrayValue = new
                            {
                                values = g.Specs.Select(s => new
                                {
                                    mapValue = new
                                    {
                                        fields = new Dictionary<string, object>
                                        {
                                            ["key"] = new { stringValue = s.Key },
                                            ["val"] = new
                                            {
                                                arrayValue = new
                                                {
                                                    values = s.Val.Select(v => new { stringValue = v }).ToArray()
                                                }
                                            }
                                        }
                                    }
                                }).ToArray()
                            }
                        }
                    }
                }
            }).ToArray()
        }
    };

    // ═══════════════════════════════════════════════════════════════════════
    //  DOCUMENT PARSERS
    // ═══════════════════════════════════════════════════════════════════════

    private static Brand? ParseBrand(JsonElement doc)
    {
        try
        {
            var fields = doc.GetProperty("fields");
            return new Brand
            {
                Id   = int.TryParse(GetStringField(fields, "id"), out var id) ? id : 0,
                Name = GetStringField(fields, "name"),
                Slug = GetStringField(fields, "slug")
            };
        }
        catch { return null; }
    }

    private static Phone? ParsePhone(JsonElement doc)
    {
        try
        {
            var fields = doc.GetProperty("fields");
            return new Phone
            {
                Id        = int.TryParse(GetStringField(fields, "id"), out var id) ? id : 0,
                Name      = GetStringField(fields, "name"),
                Slug      = GetStringField(fields, "slug"),
                BrandName = GetStringField(fields, "brandName"),
                ImageUrl  = GetStringField(fields, "imageUrl")
            };
        }
        catch { return null; }
    }

    private static PhoneDetail? ParsePhoneDetail(JsonElement doc)
    {
        try
        {
            var fields = doc.GetProperty("fields");
            var detail = new PhoneDetail
            {
                Slug     = GetStringField(fields, "slug"),
                Name     = GetStringField(fields, "name"),
                ImageUrl = GetStringField(fields, "imageUrl"),
                Os       = GetStringField(fields, "os"),
                Storage  = GetStringField(fields, "storage"),
                Specifications = ParseSpecsArray(fields)
            };
            return detail;
        }
        catch { return null; }
    }

    private static List<SpecGroup> ParseSpecsArray(JsonElement fields)
    {
        var groups = new List<SpecGroup>();
        try
        {
            if (!fields.TryGetProperty("specifications", out var specsField)) return groups;
            if (!specsField.TryGetProperty("arrayValue", out var arrVal)) return groups;
            if (!arrVal.TryGetProperty("values", out var values)) return groups;

            foreach (var gEl in values.EnumerateArray())
            {
                var gFields = gEl.GetProperty("mapValue").GetProperty("fields");
                var title = GetStringField(gFields, "title");

                var specs = new List<SpecItem>();
                if (gFields.TryGetProperty("specs", out var specsArr) &&
                    specsArr.TryGetProperty("arrayValue", out var sArrVal) &&
                    sArrVal.TryGetProperty("values", out var sValues))
                {
                    foreach (var sEl in sValues.EnumerateArray())
                    {
                        var sFields = sEl.GetProperty("mapValue").GetProperty("fields");
                        var key = GetStringField(sFields, "key");
                        var val = new List<string>();

                        if (sFields.TryGetProperty("val", out var valField) &&
                            valField.TryGetProperty("arrayValue", out var vArrVal) &&
                            vArrVal.TryGetProperty("values", out var vValues))
                        {
                            foreach (var v in vValues.EnumerateArray())
                            {
                                if (v.TryGetProperty("stringValue", out var sv))
                                    val.Add(sv.GetString() ?? "");
                            }
                        }

                        specs.Add(new SpecItem { Key = key, Val = val });
                    }
                }

                groups.Add(new SpecGroup { Title = title, Specs = specs });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firestore] ParseSpecs error: {ex.Message}");
        }
        return groups;
    }

    private static string GetStringField(JsonElement fields, string name)
    {
        if (!fields.TryGetProperty(name, out var prop)) return string.Empty;
        if (prop.TryGetProperty("stringValue", out var sv)) return sv.GetString() ?? string.Empty;
        if (prop.TryGetProperty("integerValue", out var iv)) return iv.GetString() ?? "0";
        return string.Empty;
    }
}
