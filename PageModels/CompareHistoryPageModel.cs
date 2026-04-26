using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Models;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class CompareHistoryPageModel : ObservableObject
{
    private readonly CompareHistoryService _historyService;
    private readonly IAuthService _auth;

    [ObservableProperty] private List<CompareHistory> _items = [];
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public bool HasItems => Items.Count > 0;
    public bool NoItems => !HasItems && !IsBusy;

    public CompareHistoryPageModel(CompareHistoryService historyService, IAuthService auth)
    {
        _historyService = historyService;
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
    private async Task ViewComparison(CompareHistory item)
    {
        if (item == null) return;
        await Shell.Current.GoToAsync(
            $"compare?slug1={item.Phone1Slug}&name1={Uri.EscapeDataString(item.Phone1Name)}&slug2={item.Phone2Slug}&name2={Uri.EscapeDataString(item.Phone2Name)}");
    }

    [RelayCommand]
    private async Task DeleteItem(CompareHistory item)
    {
        if (item == null) return;
        var user = _auth.GetCurrentUser();
        if (user == null) return;

        var deleted = await _historyService.DeleteHistoryAsync(item.Id, user.IdToken);
        if (deleted)
        {
            Items = new List<CompareHistory>(Items.Where(h => h.Id != item.Id));
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(NoItems));
        }
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task LoadAsync()
    {
        var user = _auth.GetCurrentUser();
        if (user == null)
        {
            StatusMessage = "Please log in to see your compare history.";
            return;
        }

        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            Items = await _historyService.GetHistoryAsync(user.Id, user.IdToken);
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(NoItems));
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
