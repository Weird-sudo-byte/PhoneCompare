using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PhoneCompare.Config;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

public class CompareHistoryService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    
    private static readonly string LocalFilePath = 
        Path.Combine(FileSystem.AppDataDirectory, "compare_history.json");

    public CompareHistoryService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CompareHistory>> GetHistoryAsync(string userId, string idToken)
    {
        try
        {
            var url = $"{FirebaseConfig.FirestoreBaseUrl}/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/compare_history?pageSize=50";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[CompareHistory] Firestore error: {json}");
                return await GetLocalHistoryAsync(userId);
            }

            var doc = JsonDocument.Parse(json);
            var history = new List<CompareHistory>();
            
            if (!doc.RootElement.TryGetProperty("documents", out var docs))
                return [];

            foreach (var d in docs.EnumerateArray())
            {
                var item = ParseDocument(d);
                if (item != null && item.UserId == userId)
                    history.Add(item);
            }

            return history.OrderByDescending(h => h.ComparedAt).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CompareHistory] Error: {ex.Message}");
            return await GetLocalHistoryAsync(userId);
        }
    }

    public async Task<bool> AddHistoryAsync(CompareHistory history, string idToken)
    {
        try
        {
            var url = $"{FirebaseConfig.FirestoreBaseUrl}/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/compare_history";
            var body = BuildFirestoreDocument(history);
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var response = await _http.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[CompareHistory] Add failed: {responseBody}");
                return await AddLocalHistoryAsync(history);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CompareHistory] Add error: {ex.Message}");
            return await AddLocalHistoryAsync(history);
        }
    }

    public async Task<bool> DeleteHistoryAsync(string historyId, string idToken)
    {
        try
        {
            var url = $"{FirebaseConfig.FirestoreBaseUrl}/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/compare_history/{historyId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CompareHistory] Delete error: {ex.Message}");
            return false;
        }
    }

    private static object BuildFirestoreDocument(CompareHistory h) => new
    {
        fields = new
        {
            userId = new { stringValue = h.UserId },
            phone1Slug = new { stringValue = h.Phone1Slug },
            phone1Name = new { stringValue = h.Phone1Name },
            phone1ImageUrl = new { stringValue = h.Phone1ImageUrl },
            phone2Slug = new { stringValue = h.Phone2Slug },
            phone2Name = new { stringValue = h.Phone2Name },
            phone2ImageUrl = new { stringValue = h.Phone2ImageUrl },
            comparedAt = new { timestampValue = h.ComparedAt.ToString("O") }
        }
    };

    private static CompareHistory? ParseDocument(JsonElement doc)
    {
        try
        {
            var name = doc.GetProperty("name").GetString() ?? string.Empty;
            var docId = name.Split('/').Last();
            var fields = doc.GetProperty("fields");

            return new CompareHistory
            {
                Id = docId,
                UserId = fields.GetProperty("userId").GetProperty("stringValue").GetString() ?? string.Empty,
                Phone1Slug = fields.GetProperty("phone1Slug").GetProperty("stringValue").GetString() ?? string.Empty,
                Phone1Name = fields.GetProperty("phone1Name").GetProperty("stringValue").GetString() ?? string.Empty,
                Phone1ImageUrl = fields.GetProperty("phone1ImageUrl").GetProperty("stringValue").GetString() ?? string.Empty,
                Phone2Slug = fields.GetProperty("phone2Slug").GetProperty("stringValue").GetString() ?? string.Empty,
                Phone2Name = fields.GetProperty("phone2Name").GetProperty("stringValue").GetString() ?? string.Empty,
                Phone2ImageUrl = fields.GetProperty("phone2ImageUrl").GetProperty("stringValue").GetString() ?? string.Empty,
                ComparedAt = DateTime.TryParse(
                    fields.GetProperty("comparedAt").GetProperty("timestampValue").GetString(), 
                    out var dt) ? dt : DateTime.UtcNow
            };
        }
        catch { return null; }
    }

    private Task<List<CompareHistory>> GetLocalHistoryAsync(string userId)
    {
        try
        {
            if (!File.Exists(LocalFilePath))
                return Task.FromResult(new List<CompareHistory>());

            var json = File.ReadAllText(LocalFilePath);
            var all = JsonSerializer.Deserialize<List<CompareHistory>>(json, JsonOpts) ?? [];
            return Task.FromResult(all.Where(h => h.UserId == userId).OrderByDescending(h => h.ComparedAt).ToList());
        }
        catch
        {
            return Task.FromResult(new List<CompareHistory>());
        }
    }

    private Task<bool> AddLocalHistoryAsync(CompareHistory history)
    {
        try
        {
            var all = new List<CompareHistory>();
            if (File.Exists(LocalFilePath))
            {
                var json = File.ReadAllText(LocalFilePath);
                all = JsonSerializer.Deserialize<List<CompareHistory>>(json, JsonOpts) ?? [];
            }

            history.Id = Guid.NewGuid().ToString("N");
            all.Add(history);

            File.WriteAllText(LocalFilePath, JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true }));
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
