using System.Text.Json;

namespace PhoneCompare.Services;

public class PhoneDataCache
{
    private static readonly string CacheDir =
        Path.Combine(FileSystem.AppDataDirectory, "phone_cache");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public PhoneDataCache()
    {
        if (!Directory.Exists(CacheDir))
            Directory.CreateDirectory(CacheDir);
    }

    public T? TryGet<T>(string key, TimeSpan maxAge) where T : class
    {
        var path = Path.Combine(CacheDir, $"{key}.json");
        if (!File.Exists(path)) return null;

        var info = new FileInfo(path);
        if (DateTime.UtcNow - info.LastWriteTimeUtc > maxAge)
        {
            File.Delete(path);
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }
        catch
        {
            File.Delete(path);
            return null;
        }
    }

    public void Set<T>(string key, T data)
    {
        var path = Path.Combine(CacheDir, $"{key}.json");
        var json = JsonSerializer.Serialize(data, JsonOpts);
        File.WriteAllText(path, json);
    }

    public void Invalidate(string key)
    {
        var path = Path.Combine(CacheDir, $"{key}.json");
        if (File.Exists(path)) File.Delete(path);
    }

    public void InvalidateAll()
    {
        if (Directory.Exists(CacheDir))
            Directory.Delete(CacheDir, recursive: true);
        Directory.CreateDirectory(CacheDir);
    }
}
