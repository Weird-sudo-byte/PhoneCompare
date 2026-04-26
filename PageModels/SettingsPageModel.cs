using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class SettingsPageModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty] private bool _isDarkMode;
    [ObservableProperty] private bool _notificationsEnabled;
    [ObservableProperty] private bool _autoClearAfterCompare;
    [ObservableProperty] private string _appVersion = "1.0.0";
    [ObservableProperty] private string _selectedSortOrder = "Popularity";
    [ObservableProperty] private string _userEmail = string.Empty;
    [ObservableProperty] private bool _isLoggedIn;

    public List<string> SortOrders { get; } = ["Popularity", "A–Z", "Release Date"];

    public SettingsPageModel(IAuthService auth)
    {
        _auth = auth;
        LoadSettings();
    }

    private void LoadSettings()
    {
        IsDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
        NotificationsEnabled = Preferences.Get("notifications_enabled", true);
        AutoClearAfterCompare = Preferences.Get("auto_clear_compare", false);
        SelectedSortOrder = Preferences.Get("default_sort", "Popularity");

        var user = _auth.GetCurrentUser();
        IsLoggedIn = user != null;
        UserEmail = user?.Email ?? string.Empty;
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            Preferences.Set("app_theme", value ? "dark" : "light");
        }
    }

    partial void OnNotificationsEnabledChanged(bool value)
    {
        Preferences.Set("notifications_enabled", value);
    }

    partial void OnAutoClearAfterCompareChanged(bool value)
    {
        Preferences.Set("auto_clear_compare", value);
    }

    partial void OnSelectedSortOrderChanged(string value)
    {
        Preferences.Set("default_sort", value);
    }

    [RelayCommand]
    private async Task ClearCache()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Clear Cache",
            "This will clear all cached data. You may need to re-download some content. Continue?",
            "Clear", "Cancel");

        if (confirm)
        {
            try
            {
                var cacheDir = FileSystem.CacheDirectory;
                if (Directory.Exists(cacheDir))
                {
                    foreach (var file in Directory.GetFiles(cacheDir))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
                await Shell.Current.DisplayAlertAsync("Done", "Cache cleared successfully.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to clear cache: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task ClearCompareHistory()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Clear Compare History",
            "This will delete all your comparison history. Continue?",
            "Clear", "Cancel");

        if (confirm)
        {
            try
            {
                var historyFile = Path.Combine(FileSystem.AppDataDirectory, "compare_history.json");
                if (File.Exists(historyFile))
                    File.Delete(historyFile);
                await Shell.Current.DisplayAlertAsync("Done", "Compare history cleared.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task ClearFavorites()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Clear Favorites",
            "This will remove all your saved phones. Continue?",
            "Clear", "Cancel");

        if (confirm)
        {
            try
            {
                var favFile = Path.Combine(FileSystem.AppDataDirectory, "favorites.json");
                if (File.Exists(favFile))
                    File.Delete(favFile);
                await Shell.Current.DisplayAlertAsync("Done", "Favorites cleared.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private static async Task ViewWalkthrough()
    {
        Preferences.Set("onboarding_seen", false);
        await Shell.Current.GoToAsync("//onboarding");
    }

    [RelayCommand]
    private async Task RateApp()
    {
        try
        {
            await Launcher.OpenAsync("https://play.google.com/store/apps/details?id=com.phonecompare");
        }
        catch
        {
            await Shell.Current.DisplayAlertAsync("Info", "Could not open the store.", "OK");
        }
    }

    [RelayCommand]
    private async Task OpenPrivacyPolicy()
    {
        await Browser.OpenAsync("https://phonecompare.app/privacy", BrowserLaunchMode.SystemPreferred);
    }

    [RelayCommand]
    private async Task Logout()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync("Sign Out", "Are you sure you want to sign out?", "Sign Out", "Cancel");
        if (!confirm) return;
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}
