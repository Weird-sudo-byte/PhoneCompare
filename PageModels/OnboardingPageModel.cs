using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PhoneCompare.PageModels;

public partial class OnboardingPageModel : ObservableObject
{
    [ObservableProperty] private int _currentIndex;
    [ObservableProperty] private bool _isLastSlide;
    [ObservableProperty] private bool _isNotLastSlide = true;
    [ObservableProperty] private bool _isFirstSlide = true;
    [ObservableProperty] private bool _isNotFirstSlide;
    [ObservableProperty] private bool _isMiddleSlide;

    public List<OnboardingSlide> Slides { get; } =
    [
        new OnboardingSlide("📱", "Welcome to PhoneCompare", "Your ultimate tool for discovering and comparing the latest smartphones.", "#E84C3D"),
        new OnboardingSlide("🏠", "Browse Phones", "Explore thousands of phones filtered by category, brand, or specs — all in one place.", "#3498DB"),
        new OnboardingSlide("🔍", "Search Anything", "Find any phone instantly by name, spec, or feature. The perfect phone is one tap away.", "#27AE60"),
        new OnboardingSlide("⚖️", "Compare Side by Side", "Select 2 phones from any screen and get a full spec-by-spec breakdown with winners highlighted.", "#F39C12"),
        new OnboardingSlide("❤️", "Save Your Picks", "Bookmark phones you love and access them anytime from the Saved tab.", "#9B59B6"),
    ];

    partial void OnCurrentIndexChanged(int value)
    {
        IsLastSlide = value == Slides.Count - 1;
        IsNotLastSlide = !IsLastSlide;
        IsFirstSlide = value == 0;
        IsNotFirstSlide = !IsFirstSlide;
        IsMiddleSlide = IsNotFirstSlide && IsNotLastSlide;
    }

    [RelayCommand]
    private async Task Next()
    {
        if (IsLastSlide)
            await Finish();
        else
            CurrentIndex++;
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentIndex > 0)
            CurrentIndex--;
    }

    [RelayCommand]
    private async Task Skip()
    {
        await Finish();
    }

    private static async Task Finish()
    {
        Preferences.Set("onboarding_seen", true);
        await Shell.Current.GoToAsync("//login");
    }
}

public class OnboardingSlide
{
    public string Icon { get; }
    public string Title { get; }
    public string Description { get; }
    public string AccentColor { get; }

    public OnboardingSlide(string icon, string title, string description, string accentColor)
    {
        Icon = icon;
        Title = title;
        Description = description;
        AccentColor = accentColor;
    }

    public Color Accent => Color.FromArgb(AccentColor);
    public Color AccentLight => Color.FromArgb(AccentColor).WithAlpha(0.15f);
}
