using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class SeedDataPageModel : ObservableObject
{
    private readonly PhoneDataSeeder _seeder;
    private readonly FirestorePhoneService _firestore;
    private readonly PhoneDataCache _cache;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _statusMessage = "Ready to seed data.";
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _resultSummary = string.Empty;
    [ObservableProperty] private bool _hasResult;

    // Stats
    [ObservableProperty] private int _dbBrands;
    [ObservableProperty] private int _dbPhones;
    [ObservableProperty] private int _dbDetails;
    [ObservableProperty] private bool _hasStats;

    private CancellationTokenSource? _cts;

    public SeedDataPageModel(PhoneDataSeeder seeder, FirestorePhoneService firestore, PhoneDataCache cache)
    {
        _seeder = seeder;
        _firestore = firestore;
        _cache = cache;

        _seeder.OnProgress += msg => MainThread.BeginInvokeOnMainThread(() => StatusMessage = msg);
        _seeder.OnProgressPercent += (cur, total) => MainThread.BeginInvokeOnMainThread(() =>
        {
            Progress = total > 0 ? (double)cur / total : 0;
        });
    }

    [RelayCommand]
    private async Task SeedMockData()
    {
        if (IsBusy) return;
        IsBusy = true;
        HasResult = false;
        Progress = 0;
        StatusMessage = "Starting seed...";
        _cts = new CancellationTokenSource();

        try
        {
            var (brands, phones, details, errors) = await _seeder.SeedAllAsync(_cts.Token);

            ResultSummary = $"✅ {brands} brands, {phones} phones, {details} details uploaded.";
            if (errors.Count > 0)
                ResultSummary += $"\n⚠️ {errors.Count} errors:\n" + string.Join("\n", errors.Take(5));

            HasResult = true;
            await RefreshStats();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            ResultSummary = $"❌ Seeding failed: {ex.Message}";
            HasResult = true;
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void CancelSeed()
    {
        _cts?.Cancel();
        StatusMessage = "Cancelling...";
    }

    [RelayCommand]
    private async Task RefreshStats()
    {
        try
        {
            var (brands, phones, details) = await _firestore.GetStatsAsync();
            DbBrands = brands;
            DbPhones = phones;
            DbDetails = details;
            HasStats = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Stats error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearCache()
    {
        _cache.InvalidateAll();
        StatusMessage = "Local cache cleared.";
    }

    [RelayCommand]
    private async Task Appearing()
    {
        await RefreshStats();
    }
}
