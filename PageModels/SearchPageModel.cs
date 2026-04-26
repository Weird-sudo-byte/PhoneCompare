using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Models;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class SearchPageModel : ObservableObject
{
    private readonly IPhoneApiService _phoneApi;
    private readonly CompareStateService _compareState;
    private readonly RecentActivityService _recentActivity;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private List<Phone> _results = [];
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasSearched;
    [ObservableProperty] private List<string> _recentSearches = [];
    [ObservableProperty] private List<Brand> _brands = [];
    [ObservableProperty] private Brand? _selectedBrand;
    [ObservableProperty] private string _selectedBudget = string.Empty;

    public bool HasResults => Results.Count > 0;
    public bool NoResults => HasSearched && Results.Count == 0 && !IsBusy;
    public bool HasRecentSearches => RecentSearches.Count > 0 && !HasSearched;
    public bool HasActiveFilters => SelectedBrand != null || !string.IsNullOrEmpty(SelectedBudget);

    public SearchPageModel(IPhoneApiService phoneApi, CompareStateService compareState, RecentActivityService recentActivity)
    {
        _phoneApi = phoneApi;
        _compareState = compareState;
        _recentActivity = recentActivity;
        LoadRecentSearches();
        LoadBrands();
    }

    private void LoadBrands()
    {
        Brands = MockPhoneData.GetBrands();
    }

    partial void OnSelectedBrandChanged(Brand? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        _ = ApplyFilters();
    }

    private void LoadRecentSearches()
    {
        RecentSearches = _recentActivity.GetRecentSearches();
        OnPropertyChanged(nameof(HasRecentSearches));
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || IsBusy) return;
        
        IsBusy = true;
        HasSearched = true;
        try
        {
            var query = SearchQuery.Trim();
            _recentActivity.AddRecentSearch(query);
            LoadRecentSearches();
            
            Results = await _phoneApi.SearchPhonesAsync(query);
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(NoResults));
        }
        catch { Results = []; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private static async Task BrowsePhones() => await Shell.Current.GoToAsync("//phones");

    [RelayCommand]
    private async Task SearchFromRecent(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;
        SearchQuery = query;
        await Search();
    }

    [RelayCommand]
    private async Task ViewPhone(Phone phone)
    {
        if (phone == null) return;
        await Shell.Current.GoToAsync($"detail?slug={phone.Slug}&name={Uri.EscapeDataString(phone.Name)}");
    }

    [RelayCommand]
    private void ToggleCompare(Phone phone)
    {
        _compareState.Toggle(phone);
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        Results = [];
        HasSearched = false;
        SelectedBrand = null;
        SelectedBudget = string.Empty;
        LoadRecentSearches();
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(NoResults));
        OnPropertyChanged(nameof(HasRecentSearches));
    }

    [RelayCommand]
    private async Task FilterByBudget(string budget)
    {
        SelectedBudget = budget;
        OnPropertyChanged(nameof(HasActiveFilters));
        await ApplyFilters();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedBrand = null;
        SelectedBudget = string.Empty;
        Results = [];
        HasSearched = false;
        OnPropertyChanged(nameof(HasActiveFilters));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(NoResults));
        OnPropertyChanged(nameof(HasRecentSearches));
    }

    private async Task ApplyFilters()
    {
        IsBusy = true;
        HasSearched = true;
        try
        {
            var allPhones = MockPhoneData.GetPhones();
            var filtered = allPhones.AsEnumerable();

            // Apply brand filter
            if (SelectedBrand != null)
            {
                filtered = filtered.Where(p => p.BrandName.Equals(SelectedBrand.Name, StringComparison.OrdinalIgnoreCase));
            }

            // Apply budget filter
            if (!string.IsNullOrEmpty(SelectedBudget))
            {
                var parts = SelectedBudget.Split('-');
                if (parts.Length == 2 && decimal.TryParse(parts[0], out var min) && decimal.TryParse(parts[1], out var max))
                {
                    filtered = filtered.Where(p => p.PricePHP >= min && p.PricePHP <= max);
                }
            }

            // Apply search query
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.Trim().ToLowerInvariant();
                filtered = filtered.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            Results = filtered.ToList();
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(NoResults));
        }
        catch { Results = []; }
        finally { IsBusy = false; }
    }
}
