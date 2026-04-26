using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Models;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

[QueryProperty(nameof(Slug), "slug")]
[QueryProperty(nameof(PhoneName), "name")]
public partial class PhoneDetailPageModel : ObservableObject
{
    private readonly IPhoneApiService _phoneApi;
    private readonly IAuthService _auth;
    private readonly IFavoritesService _favorites;
    private readonly CompareStateService _compareState;
    private readonly RecentActivityService _recentActivity;

    [ObservableProperty] private string _slug = string.Empty;
    [ObservableProperty] private string _phoneName = string.Empty;
    [ObservableProperty] private PhoneDetail? _phone;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _phonePriceDisplay = string.Empty;
    [ObservableProperty] private List<Phone> _similarPhones = [];
    [ObservableProperty] private List<RetailerPrice> _retailers = [];

    public bool HasPhone => Phone != null;
    public bool HasSimilarPhones => SimilarPhones.Count > 0;
    public bool HasRetailers => Retailers.Count > 0;

    public PhoneDetailPageModel(IPhoneApiService phoneApi, IAuthService auth, IFavoritesService favorites, CompareStateService compareState, RecentActivityService recentActivity)
    {
        _phoneApi = phoneApi;
        _auth = auth;
        _favorites = favorites;
        _compareState = compareState;
        _recentActivity = recentActivity;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        if (string.IsNullOrEmpty(Slug)) return;
        await LoadPhoneAsync();
    }

    [RelayCommand]
    private async Task ToggleFavorite()
    {
        var user = _auth.GetCurrentUser();
        if (user == null || Phone == null) return;

        if (IsFavorite)
        {
            var favs = await _favorites.GetFavoritesAsync(user.Id, user.IdToken);
            var fav = favs.FirstOrDefault(f => f.PhoneSlug == Slug);
            if (fav != null)
            {
                await _favorites.RemoveFavoriteAsync(fav.Id, user.Id, user.IdToken);
                IsFavorite = false;
            }
        }
        else
        {
            await _favorites.AddFavoriteAsync(new Favorite
            {
                UserId = user.Id,
                PhoneSlug = Slug,
                PhoneName = Phone.Name,
                PhoneImageUrl = Phone.ImageUrl,
                SavedAt = DateTime.UtcNow
            }, user.IdToken);
            IsFavorite = true;
        }
    }

    [RelayCommand]
    private async Task AddToCompare()
    {
        if (Phone != null)
        {
            var phone = new Phone
            {
                Name = Phone.Name,
                Slug = Phone.Slug,
                ImageUrl = Phone.ImageUrl,
                BrandName = Phone.Name.Split(' ')[0]
            };
            _compareState.Toggle(phone);
        }
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Share()
    {
        if (Phone == null) return;
        
        var deepLink = $"phonecompare://phone/{Slug}";
        var shareText = $"📱 {Phone.Name}\n\n" +
                        $"💰 {PhonePriceDisplay}\n" +
                        $"📷 Camera: {Phone.MainCamera}\n" +
                        $"🔋 Battery: {Phone.Battery}\n" +
                        $"💾 Storage: {Phone.Ram}\n\n" +
                        $"Check it out on PhoneCompare!\n{deepLink}";
        
        await Microsoft.Maui.ApplicationModel.DataTransfer.Share.RequestAsync(new ShareTextRequest
        {
            Text = shareText,
            Title = $"Share {Phone.Name}"
        });
    }

    [RelayCommand]
    private async Task OpenRetailer(RetailerPrice retailer)
    {
        if (retailer == null || string.IsNullOrEmpty(retailer.Url)) return;
        
        try
        {
            await Browser.OpenAsync(retailer.Url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OpenRetailer] Failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewSimilarPhone(Phone phone)
    {
        if (phone == null) return;
        await Shell.Current.GoToAsync($"detail?slug={phone.Slug}&name={Uri.EscapeDataString(phone.Name)}");
    }

    private async Task LoadPhoneAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            Phone = await _phoneApi.GetPhoneDetailAsync(Slug);
            OnPropertyChanged(nameof(HasPhone));

            // Get PHP price from mock data
            var phoneData = MockPhoneData.GetPhones().FirstOrDefault(p => p.Slug == Slug);
            PhonePriceDisplay = phoneData?.PriceDisplay ?? "Price TBA";

            // Track recently viewed and load additional data
            if (Phone != null)
            {
                _recentActivity.AddRecentlyViewed(Slug, Phone.Name, Phone.ImageUrl);
                await LoadSimilarPhonesAsync(phoneData);
                
                // Load retailer prices
                Retailers = MockPhoneData.GetRetailerPrices(Slug, phoneData?.PricePHP ?? 0);
                OnPropertyChanged(nameof(HasRetailers));
            }

            var user = _auth.GetCurrentUser();
            if (user != null && Phone != null)
                IsFavorite = await _favorites.IsFavoriteAsync(user.Id, Slug, user.IdToken);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task LoadSimilarPhonesAsync(Phone? currentPhone)
    {
        try
        {
            var allPhones = MockPhoneData.GetPhones();
            var brandName = Phone?.Name.Split(' ')[0] ?? "";
            var currentPrice = currentPhone?.PricePHP ?? 0;
            
            // Find similar phones: same brand OR similar price (±25%)
            var similar = allPhones
                .Where(p => p.Slug != Slug)
                .Where(p => 
                    p.Name.StartsWith(brandName, StringComparison.OrdinalIgnoreCase) ||
                    (currentPrice > 0 && p.PricePHP > 0 && 
                     p.PricePHP >= currentPrice * 0.75m && 
                     p.PricePHP <= currentPrice * 1.25m))
                .Take(6)
                .ToList();
            
            SimilarPhones = similar;
            OnPropertyChanged(nameof(HasSimilarPhones));
        }
        catch { SimilarPhones = []; }
        await Task.CompletedTask;
    }
}
