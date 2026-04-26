using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Models;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class FavoritesPageModel : ObservableObject
{
    private readonly IFavoritesService _favorites;
    private readonly IAuthService _auth;

    [ObservableProperty] private List<Favorite> _items = [];
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public bool HasItems => Items.Count > 0;
    public bool NoItems => !HasItems && !IsBusy;

    public FavoritesPageModel(IFavoritesService favorites, IAuthService auth)
    {
        _favorites = favorites;
        _auth = auth;
    }

    [RelayCommand]
    private async Task Appearing() => await LoadAsync();

    [RelayCommand]
    private static async Task BrowsePhones() => await Shell.Current.GoToAsync("//phones");

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        await LoadAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task ViewPhone(Favorite fav)
    {
        if (fav == null) return;
        await Shell.Current.GoToAsync($"detail?slug={fav.PhoneSlug}&name={Uri.EscapeDataString(fav.PhoneName)}");
    }

    [RelayCommand]
    private async Task RemoveFavorite(Favorite fav)
    {
        if (fav == null) return;
        var user = _auth.GetCurrentUser();
        if (user == null) return;

        var removed = await _favorites.RemoveFavoriteAsync(fav.Id, user.Id, user.IdToken);
        if (removed)
        {
            Items = new List<Favorite>(Items.Where(f => f.Id != fav.Id));
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(NoItems));
        }
    }

    private async Task LoadAsync()
    {
        var user = _auth.GetCurrentUser();
        if (user == null)
        {
            StatusMessage = "Please log in to see your favorites.";
            return;
        }

        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            Items = await _favorites.GetFavoritesAsync(user.Id, user.IdToken);
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(NoItems));
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
