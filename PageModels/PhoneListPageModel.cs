using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Models;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class PhoneListPageModel : ObservableObject
{
    private readonly IPhoneApiService _phoneApi;
    private readonly IAuthService _auth;
    private readonly CompareStateService _compareState;
    private readonly IFavoritesService _favorites;
    private readonly RecentActivityService _recentActivity;

    [ObservableProperty] private List<Phone> _phones = [];
    [ObservableProperty] private List<Brand> _brands = [];
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private Brand? _selectedBrand;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _activeFilter = "Latest";
    [ObservableProperty] private List<RecentPhone> _recentlyViewed = [];

    private bool _initialized;

    public List<Phone> CompareList => _compareState.CompareList;
    public bool CanCompare => _compareState.CanCompare;
    public int CompareCount => _compareState.CompareCount;
    public bool HasPhones => Phones.Count > 0;
    public bool NoPhones => !HasPhones && !IsBusy;
    public bool HasRecentlyViewed => RecentlyViewed.Count > 0;

    public PhoneListPageModel(IPhoneApiService phoneApi, IAuthService auth, CompareStateService compareState, IFavoritesService favorites, RecentActivityService recentActivity)
    {
        _phoneApi = phoneApi;
        _auth = auth;
        _compareState = compareState;
        _favorites = favorites;
        _recentActivity = recentActivity;
        _compareState.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CompareStateService.CompareList))
            {
                OnPropertyChanged(nameof(CompareList));
                OnPropertyChanged(nameof(CanCompare));
                OnPropertyChanged(nameof(CompareCount));
            }
        };
    }

    [RelayCommand]
    private async Task Appearing()
    {
        // Always refresh recently viewed on appearing
        RecentlyViewed = _recentActivity.GetRecentlyViewed();
        OnPropertyChanged(nameof(HasRecentlyViewed));
        
        if (!_initialized)
        {
            _initialized = true;
            await LoadBrandsAsync();
            await LoadLatestAsync();
        }
    }

    [RelayCommand]
    private async Task GoToQuiz() => await Shell.Current.GoToAsync("quiz");

    [RelayCommand]
    private async Task ViewRecentPhone(RecentPhone phone)
    {
        if (phone == null) return;
        await Shell.Current.GoToAsync($"detail?slug={phone.Slug}&name={Uri.EscapeDataString(phone.Name)}");
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        IsBusy = true;
        StatusMessage = string.Empty;
        ActiveFilter = "Search";
        try
        {
            Phones = await _phoneApi.SearchPhonesAsync(SearchQuery);
            NotifyList();
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task LoadLatest()
    {
        ActiveFilter = "Latest";
        SelectedBrand = null;
        await LoadLatestAsync();
    }

    [RelayCommand]
    private async Task LoadTop()
    {
        ActiveFilter = "Top";
        SelectedBrand = null;
        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            Phones = await _phoneApi.GetTopPhonesAsync();
            NotifyList();
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task FilterByBudget(string budgetRange)
    {
        if (string.IsNullOrEmpty(budgetRange)) return;
        
        var parts = budgetRange.Split('-');
        if (parts.Length != 2) return;
        
        if (!decimal.TryParse(parts[0], out var minPrice) || !decimal.TryParse(parts[1], out var maxPrice)) return;

        IsBusy = true;
        StatusMessage = string.Empty;
        
        // Set active filter label
        if (minPrice == 0)
            ActiveFilter = $"Under ₱{maxPrice/1000:N0}K";
        else if (maxPrice >= 999999)
            ActiveFilter = $"Over ₱{minPrice/1000:N0}K";
        else
            ActiveFilter = $"₱{minPrice/1000:N0}K-{maxPrice/1000:N0}K";

        SelectedBrand = null;

        try
        {
            var allPhones = await _phoneApi.GetLatestPhonesAsync();
            Phones = allPhones.Where(p => p.PricePHP >= minPrice && p.PricePHP <= maxPrice).ToList();
            NotifyList();
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task FilterByCategory(string category)
    {
        if (string.IsNullOrEmpty(category)) return;

        IsBusy = true;
        StatusMessage = string.Empty;
        SelectedBrand = null;

        // Set active filter label with emoji
        ActiveFilter = category switch
        {
            "camera" => "📷 Camera",
            "gaming" => "🎮 Gaming",
            "battery" => "🔋 Battery",
            "value" => "💰 Value",
            "selfie" => "🤳 Selfie",
            _ => category
        };

        try
        {
            var allPhones = await _phoneApi.GetLatestPhonesAsync();
            
            // Filter based on category using phone name patterns
            // Since we don't have detailed specs in the list view, use naming conventions
            Phones = category switch
            {
                "camera" => allPhones.Where(p => 
                    p.Name.Contains("Ultra", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Pro", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Camera", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("200MP", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("108MP", StringComparison.OrdinalIgnoreCase)).ToList(),
                    
                "gaming" => allPhones.Where(p =>
                    p.Name.Contains("ROG", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Gaming", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Legion", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Red Magic", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Black Shark", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("POCO", StringComparison.OrdinalIgnoreCase)).ToList(),
                    
                "battery" => allPhones.Where(p =>
                    p.Name.Contains("Max", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Power", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Energy", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("6000", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("7000", StringComparison.OrdinalIgnoreCase)).ToList(),
                    
                "value" => allPhones.Where(p => 
                    p.PricePHP > 0 && p.PricePHP <= 25000).ToList(),
                    
                "selfie" => allPhones.Where(p =>
                    p.Name.Contains("Camon", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("V", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Nova", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Reno", StringComparison.OrdinalIgnoreCase)).ToList(),
                    
                _ => allPhones
            };
            
            NotifyList();
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SelectBrand(Brand brand)
    {
        if (brand == null) return;
        await LoadBrandPhonesAsync(brand);
    }

    [RelayCommand]
    private async Task BrandSelected(Brand? brand)
    {
        System.Diagnostics.Debug.WriteLine($"[BrandSelected] Brand param: {brand?.Name ?? "null"}, SelectedBrand: {SelectedBrand?.Name ?? "null"}");
        var targetBrand = brand ?? SelectedBrand;
        if (targetBrand != null)
        {
            await LoadBrandPhonesAsync(targetBrand);
        }
    }

    private async Task LoadBrandPhonesAsync(Brand brand)
    {
        System.Diagnostics.Debug.WriteLine($"[LoadBrandPhones] START - Brand: {brand.Name}, Slug: {brand.Slug}");
        ActiveFilter = brand.Name;
        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            System.Diagnostics.Debug.WriteLine($"[LoadBrandPhones] Calling API...");
            var phones = await _phoneApi.GetPhonesByBrandAsync(brand.Slug);
            System.Diagnostics.Debug.WriteLine($"[LoadBrandPhones] API returned {phones?.Count ?? 0} phones");
            
            if (phones != null && phones.Count > 0)
            {
                foreach (var p in phones) p.BrandName = brand.Name;
                Phones = phones;
                System.Diagnostics.Debug.WriteLine($"[LoadBrandPhones] Set Phones list, count: {Phones.Count}");
            }
            else
            {
                Phones = [];
                StatusMessage = $"No phones found for {brand.Name}";
                System.Diagnostics.Debug.WriteLine($"[LoadBrandPhones] No phones found");
            }
            NotifyList();
        }
        catch (Exception ex) 
        { 
            System.Diagnostics.Debug.WriteLine($"[LoadBrandPhones] ERROR: {ex.Message}");
            StatusMessage = ex.Message; 
        }
        finally { IsBusy = false; }
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
    private async Task ToggleFavorite(Phone phone)
    {
        if (phone == null) return;
        
        var user = _auth.GetCurrentUser();
        if (user == null)
        {
            StatusMessage = "Please log in to add favorites";
            return;
        }

        try
        {
            if (phone.IsFavorite)
            {
                var favs = await _favorites.GetFavoritesAsync(user.Id, user.IdToken);
                var fav = favs.FirstOrDefault(f => f.PhoneSlug == phone.Slug);
                if (fav != null)
                {
                    await _favorites.RemoveFavoriteAsync(fav.Id, user.Id, user.IdToken);
                    phone.IsFavorite = false;
                }
            }
            else
            {
                await _favorites.AddFavoriteAsync(new Favorite
                {
                    UserId = user.Id,
                    PhoneSlug = phone.Slug,
                    PhoneName = phone.Name,
                    PhoneImageUrl = phone.ImageUrl,
                    SavedAt = DateTime.UtcNow
                }, user.IdToken);
                phone.IsFavorite = true;
            }
            
            // Refresh the list to update UI
            var index = Phones.IndexOf(phone);
            if (index >= 0)
            {
                Phones = new List<Phone>(Phones);
                OnPropertyChanged(nameof(Phones));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task Compare()
    {
        if (!_compareState.CanCompare) return;
        var p1 = _compareState.CompareList[0];
        var p2 = _compareState.CompareList[1];
        await Shell.Current.GoToAsync(
            $"compare?slug1={p1.Slug}&name1={Uri.EscapeDataString(p1.Name)}&slug2={p2.Slug}&name2={Uri.EscapeDataString(p2.Name)}");
    }

    [RelayCommand]
    private void ClearCompare()
    {
        _compareState.Clear();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        try
        {
            if (SelectedBrand != null)
                await SelectBrand(SelectedBrand);
            else
                await LoadLatestAsync();
        }
        finally { IsRefreshing = false; }
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }

    private Task LoadBrandsAsync()
    {
        // Hardcoded known brands - NO API call
        Brands = new List<Brand>
        {
            new() { Name = "Apple", Slug = "apple" },
            new() { Name = "Asus", Slug = "asus" },
            new() { Name = "Google", Slug = "google" },
            new() { Name = "Honor", Slug = "honor" },
            new() { Name = "Huawei", Slug = "huawei" },
            new() { Name = "Infinix", Slug = "infinix" },
            new() { Name = "Itel", Slug = "itel" },
            new() { Name = "Motorola", Slug = "motorola" },
            new() { Name = "Nokia", Slug = "nokia" },
            new() { Name = "Nothing", Slug = "nothing" },
            new() { Name = "OnePlus", Slug = "oneplus" },
            new() { Name = "Oppo", Slug = "oppo" },
            new() { Name = "Poco", Slug = "poco" },
            new() { Name = "Realme", Slug = "realme" },
            new() { Name = "Samsung", Slug = "samsung" },
            new() { Name = "Sony", Slug = "sony" },
            new() { Name = "Tecno", Slug = "tecno" },
            new() { Name = "Vivo", Slug = "vivo" },
            new() { Name = "Xiaomi", Slug = "xiaomi" }
        };
        System.Diagnostics.Debug.WriteLine($"[Brands] Initialized {Brands.Count} hardcoded brands");
        return Task.CompletedTask;
    }

    partial void OnSelectedBrandChanged(Brand? value)
    {
        if (value != null)
        {
            System.Diagnostics.Debug.WriteLine($"[OnSelectedBrandChanged] Brand changed to: {value.Name} ({value.Slug})");
            _ = LoadBrandPhonesAsync(value);
        }
    }

    private async Task LoadLatestAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            Phones = await _phoneApi.GetLatestPhonesAsync();
            await LoadFavoriteStatesAsync();
            NotifyList();
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task LoadFavoriteStatesAsync()
    {
        var user = _auth.GetCurrentUser();
        if (user == null) return;

        try
        {
            var favs = await _favorites.GetFavoritesAsync(user.Id, user.IdToken);
            var favSlugs = favs.Select(f => f.PhoneSlug).ToHashSet();
            foreach (var phone in Phones)
            {
                phone.IsFavorite = favSlugs.Contains(phone.Slug);
            }
        }
        catch { /* Silently fail - favorites will show as not favorited */ }
    }

    private void NotifyList()
    {
        OnPropertyChanged(nameof(HasPhones));
        OnPropertyChanged(nameof(NoPhones));
    }
}
