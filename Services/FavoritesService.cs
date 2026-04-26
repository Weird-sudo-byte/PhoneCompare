using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PhoneCompare.Config;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

public class FavoritesService : IFavoritesService
{
    private readonly HttpClient _http;
    private readonly LocalFavoritesService _local;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public FavoritesService(HttpClient http, LocalFavoritesService local)
    {
        _http = http;
        _local = local;
    }

    private string CollectionUrl(string userId) =>
        $"{FirebaseConfig.FirestoreBaseUrl}/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/favorites";

    public async Task<List<Favorite>> GetFavoritesAsync(string userId, string idToken)
    {
        try
        {
            var url = $"{FirebaseConfig.FirestoreBaseUrl}/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/favorites?pageSize=100";
            System.Diagnostics.Debug.WriteLine($"[Favorites] GET {url}");
            System.Diagnostics.Debug.WriteLine($"[Favorites] Token: {idToken?.Substring(0, Math.Min(20, idToken?.Length ?? 0))}...");
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"[Favorites] Response: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[Favorites] Body: {json.Substring(0, Math.Min(500, json.Length))}");
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[Favorites] Firestore error: {json}");
                return await _local.GetFavoritesAsync(userId);
            }

            var doc = JsonDocument.Parse(json);

            var favorites = new List<Favorite>();
            if (!doc.RootElement.TryGetProperty("documents", out var docs))
            {
                System.Diagnostics.Debug.WriteLine($"[Favorites] No documents found in response");
                return [];
            }

            foreach (var d in docs.EnumerateArray())
            {
                var fav = ParseFavoriteDocument(d);
                if (fav != null && fav.UserId == userId)
                {
                    System.Diagnostics.Debug.WriteLine($"[Favorites] Found: {fav.PhoneName} (ID: {fav.Id})");
                    favorites.Add(fav);
                }
            }

            System.Diagnostics.Debug.WriteLine($"[Favorites] Total found for user: {favorites.Count}");
            return favorites;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Favorites] GetFavorites ERROR: {ex.GetType().Name} - {ex.Message}");
            return await _local.GetFavoritesAsync(userId);
        }
    }

    public async Task<bool> AddFavoriteAsync(Favorite favorite, string idToken)
    {
        try
        {
            var url = $"{FirebaseConfig.FirestoreBaseUrl}/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/favorites";
            var body = BuildFirestoreDocument(favorite);
            var bodyJson = JsonSerializer.Serialize(body);
            
            System.Diagnostics.Debug.WriteLine($"[Favorites] POST {url}");
            System.Diagnostics.Debug.WriteLine($"[Favorites] Body: {bodyJson}");
            
            var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"[Favorites] Add Response: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[Favorites] Add Body: {responseBody.Substring(0, Math.Min(300, responseBody.Length))}");
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[Favorites] Add FAILED: {responseBody}");
                return await _local.AddFavoriteAsync(favorite);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Favorites] AddFavorite ERROR: {ex.GetType().Name} - {ex.Message}");
            return await _local.AddFavoriteAsync(favorite);
        }
    }

    public async Task<bool> RemoveFavoriteAsync(string favoriteId, string userId, string idToken)
    {
        try
        {
            var url = $"{FirebaseConfig.FirestoreBaseUrl}/projects/{FirebaseConfig.ProjectId}/databases/(default)/documents/favorites/{favoriteId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Favorites] RemoveFavorite Firestore failed, using local: {ex.Message}");
            return await _local.RemoveFavoriteAsync(favoriteId, userId);
        }
    }

    public async Task<bool> IsFavoriteAsync(string userId, string phoneSlug, string idToken)
    {
        var favs = await GetFavoritesAsync(userId, idToken);
        return favs.Any(f => f.PhoneSlug == phoneSlug);
    }

    private static object BuildFirestoreDocument(Favorite fav) => new
    {
        fields = new
        {
            userId       = new { stringValue = fav.UserId },
            phoneSlug    = new { stringValue = fav.PhoneSlug },
            phoneName    = new { stringValue = fav.PhoneName },
            phoneImageUrl= new { stringValue = fav.PhoneImageUrl },
            savedAt      = new { timestampValue = fav.SavedAt.ToString("O") }
        }
    };

    private static Favorite? ParseFavoriteDocument(JsonElement doc)
    {
        try
        {
            var name   = doc.GetProperty("name").GetString() ?? string.Empty;
            var docId  = name.Split('/').Last();
            var fields = doc.GetProperty("fields");

            return new Favorite
            {
                Id           = docId,
                UserId       = fields.GetProperty("userId").GetProperty("stringValue").GetString() ?? string.Empty,
                PhoneSlug    = fields.GetProperty("phoneSlug").GetProperty("stringValue").GetString() ?? string.Empty,
                PhoneName    = fields.GetProperty("phoneName").GetProperty("stringValue").GetString() ?? string.Empty,
                PhoneImageUrl= fields.GetProperty("phoneImageUrl").GetProperty("stringValue").GetString() ?? string.Empty,
            };
        }
        catch { return null; }
    }
}
