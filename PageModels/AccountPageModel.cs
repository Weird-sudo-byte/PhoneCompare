using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class AccountPageModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty] private bool _isLoggedIn;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private string _userEmail = string.Empty;
    [ObservableProperty] private string _userName = string.Empty;

    public AccountPageModel(IAuthService auth)
    {
        _auth = auth;
        RefreshUserState();
    }

    [RelayCommand]
    private void Appearing()
    {
        RefreshUserState();
    }

    private void RefreshUserState()
    {
        var user = _auth.GetCurrentUser();
        IsLoggedIn = user != null;
        IsAdmin = user?.IsAdmin ?? false;
        UserEmail = user?.Email ?? string.Empty;
        UserName = user?.DisplayName ?? user?.Email?.Split('@')[0] ?? "User";
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _auth.LogoutAsync();
        RefreshUserState();
        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    private async Task GoToLogin()
    {
        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    private async Task GoToSeedPage()
    {
        await Shell.Current.GoToAsync("seed");
    }

    [RelayCommand]
    private async Task GoToFavorites()
    {
        await Shell.Current.GoToAsync("//favorites");
    }

    [RelayCommand]
    private async Task GoToCompareHistory()
    {
        await Shell.Current.GoToAsync("comparehistory");
    }

    [RelayCommand]
    private async Task GoToSettings()
    {
        await Shell.Current.GoToAsync("settings");
    }
}
