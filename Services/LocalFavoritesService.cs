using System.Text.Json;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

public class LocalFavoritesService
{
    private static readonly string FilePath =
        Path.Combine(FileSystem.AppDataDirectory, "favorites.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private List<Favorite>? _cached;

    private List<Favorite> Load()
    {
        if (_cached != null) return _cached;
        if (!File.Exists(FilePath)) return _cached = [];

        try
        {
            var json = File.ReadAllText(FilePath);
            _cached = JsonSerializer.Deserialize<List<Favorite>>(json, JsonOpts) ?? [];
        }
        catch
        {
            _cached = [];
        }

        return _cached;
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_cached ?? [], JsonOpts);
        File.WriteAllText(FilePath, json);
    }

    public Task<List<Favorite>> GetFavoritesAsync(string userId)
    {
        var all = Load().Where(f => f.UserId == userId).ToList();
        return Task.FromResult(all);
    }

    public Task<bool> AddFavoriteAsync(Favorite favorite)
    {
        var list = Load();
        if (list.Any(f => f.PhoneSlug == favorite.PhoneSlug && f.UserId == favorite.UserId))
            return Task.FromResult(false);

        favorite.Id = Guid.NewGuid().ToString("N");
        list.Add(favorite);
        _cached = list;
        Save();
        return Task.FromResult(true);
    }

    public Task<bool> RemoveFavoriteAsync(string favoriteId, string userId)
    {
        var list = Load();
        var item = list.FirstOrDefault(f => f.Id == favoriteId && f.UserId == userId);
        if (item == null) return Task.FromResult(false);

        list.Remove(item);
        _cached = list;
        Save();
        return Task.FromResult(true);
    }

    public Task<bool> IsFavoriteAsync(string userId, string phoneSlug)
    {
        var exists = Load().Any(f => f.UserId == userId && f.PhoneSlug == phoneSlug);
        return Task.FromResult(exists);
    }
}
