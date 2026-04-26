using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Models;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class QuizPageModel : ObservableObject
{
    // ── Step tracking ────────────────────────────────────────────────────────
    [ObservableProperty] private int _currentStep = 1; // 1=Budget 2=UseCase 3=Brand
    [ObservableProperty] private bool _showResults;
    [ObservableProperty] private bool _showQuiz = true;

    // ── Selections ───────────────────────────────────────────────────────────
    [ObservableProperty] private string _selectedBudget = string.Empty;
    [ObservableProperty] private string _selectedUseCase = string.Empty;
    [ObservableProperty] private string _selectedBrand = string.Empty;

    // ── Results ──────────────────────────────────────────────────────────────
    [ObservableProperty] private List<QuizResult> _results = [];
    [ObservableProperty] private bool _hasResults;

    // ── Step visibility ──────────────────────────────────────────────────────
    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;
    public bool IsStep3 => CurrentStep == 3;

    public double ProgressStep1 => CurrentStep >= 1 ? 1.0 : 0.0;
    public double ProgressStep2 => CurrentStep >= 2 ? 1.0 : 0.0;
    public double ProgressStep3 => CurrentStep >= 3 ? 1.0 : 0.0;

    public string StepLabel => $"Step {CurrentStep} of 3";

    // ── Budget options ───────────────────────────────────────────────────────
    public bool IsBudgetLow    => SelectedBudget == "low";
    public bool IsBudgetMid    => SelectedBudget == "mid";
    public bool IsBudgetHigh   => SelectedBudget == "high";

    // ── Use case options ─────────────────────────────────────────────────────
    public bool IsUseCaseCamera  => SelectedUseCase == "camera";
    public bool IsUseCaseGaming  => SelectedUseCase == "gaming";
    public bool IsUseCaseBattery => SelectedUseCase == "battery";
    public bool IsUseCaseValue   => SelectedUseCase == "value";

    // ── Brand options ────────────────────────────────────────────────────────
    public bool IsBrandAny      => SelectedBrand == "any";
    public bool IsBrandApple    => SelectedBrand == "Apple";
    public bool IsBrandSamsung  => SelectedBrand == "Samsung";
    public bool IsBrandXiaomi   => SelectedBrand == "Xiaomi";
    public bool IsBrandOppo     => SelectedBrand == "Oppo";
    public bool IsBrandOther    => SelectedBrand == "other";

    private void NotifyStepProps()
    {
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
        OnPropertyChanged(nameof(ProgressStep1));
        OnPropertyChanged(nameof(ProgressStep2));
        OnPropertyChanged(nameof(ProgressStep3));
        OnPropertyChanged(nameof(StepLabel));
    }

    // ── Step 1: Budget ───────────────────────────────────────────────────────
    [RelayCommand]
    private void SelectBudget(string budget)
    {
        SelectedBudget = budget;
        OnPropertyChanged(nameof(IsBudgetLow));
        OnPropertyChanged(nameof(IsBudgetMid));
        OnPropertyChanged(nameof(IsBudgetHigh));
        CurrentStep = 2;
        NotifyStepProps();
    }

    // ── Step 2: Use Case ─────────────────────────────────────────────────────
    [RelayCommand]
    private void SelectUseCase(string useCase)
    {
        SelectedUseCase = useCase;
        OnPropertyChanged(nameof(IsUseCaseCamera));
        OnPropertyChanged(nameof(IsUseCaseGaming));
        OnPropertyChanged(nameof(IsUseCaseBattery));
        OnPropertyChanged(nameof(IsUseCaseValue));
        CurrentStep = 3;
        NotifyStepProps();
    }

    // ── Step 3: Brand ────────────────────────────────────────────────────────
    [RelayCommand]
    private void SelectBrand(string brand)
    {
        SelectedBrand = brand;
        OnPropertyChanged(nameof(IsBrandAny));
        OnPropertyChanged(nameof(IsBrandApple));
        OnPropertyChanged(nameof(IsBrandSamsung));
        OnPropertyChanged(nameof(IsBrandXiaomi));
        OnPropertyChanged(nameof(IsBrandOppo));
        OnPropertyChanged(nameof(IsBrandOther));
        RunQuiz();
    }

    // ── Navigation ───────────────────────────────────────────────────────────
    [RelayCommand]
    private void Back()
    {
        if (ShowResults)
        {
            ShowResults = false;
            ShowQuiz = true;
            CurrentStep = 3;
            NotifyStepProps();
            return;
        }
        if (CurrentStep > 1)
        {
            CurrentStep--;
            NotifyStepProps();
        }
    }

    [RelayCommand]
    private void StartOver()
    {
        SelectedBudget = string.Empty;
        SelectedUseCase = string.Empty;
        SelectedBrand = string.Empty;
        CurrentStep = 1;
        ShowResults = false;
        ShowQuiz = true;
        Results = [];
        HasResults = false;
        NotifyStepProps();
        OnPropertyChanged(nameof(IsBudgetLow));
        OnPropertyChanged(nameof(IsBudgetMid));
        OnPropertyChanged(nameof(IsBudgetHigh));
        OnPropertyChanged(nameof(IsUseCaseCamera));
        OnPropertyChanged(nameof(IsUseCaseGaming));
        OnPropertyChanged(nameof(IsUseCaseBattery));
        OnPropertyChanged(nameof(IsUseCaseValue));
        OnPropertyChanged(nameof(IsBrandAny));
        OnPropertyChanged(nameof(IsBrandApple));
        OnPropertyChanged(nameof(IsBrandSamsung));
        OnPropertyChanged(nameof(IsBrandXiaomi));
        OnPropertyChanged(nameof(IsBrandOppo));
        OnPropertyChanged(nameof(IsBrandOther));
    }

    [RelayCommand]
    private async Task ViewPhone(QuizResult result)
    {
        if (result == null) return;
        await Shell.Current.GoToAsync($"detail?slug={result.Phone.Slug}&name={Uri.EscapeDataString(result.Phone.Name)}");
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    // ── Filtering & Scoring ──────────────────────────────────────────────────
    private void RunQuiz()
    {
        var phones = MockPhoneData.GetPhones();

        // Budget filter
        phones = SelectedBudget switch
        {
            "low"  => phones.Where(p => p.PricePHP > 0 && p.PricePHP < 15000).ToList(),
            "mid"  => phones.Where(p => p.PricePHP >= 15000 && p.PricePHP <= 30000).ToList(),
            "high" => phones.Where(p => p.PricePHP > 30000).ToList(),
            _      => phones
        };

        // Brand filter
        if (SelectedBrand != "any" && SelectedBrand != "other")
            phones = phones.Where(p => p.BrandName.Equals(SelectedBrand, StringComparison.OrdinalIgnoreCase)).ToList();
        else if (SelectedBrand == "other")
            phones = phones.Where(p => !new[] { "Apple", "Samsung", "Xiaomi", "Oppo" }.Contains(p.BrandName)).ToList();

        // Score each phone by use case
        var scored = phones.Select(p => new QuizResult
        {
            Phone = p,
            Score = ScorePhone(p, SelectedUseCase)
        })
        .OrderByDescending(r => r.Score)
        .Take(8)
        .ToList();

        // Mark best match
        if (scored.Count > 0)
            scored[0].IsBestMatch = true;

        // Assign rank labels
        for (int i = 0; i < scored.Count; i++)
            scored[i].Rank = i + 1;

        Results = scored;
        HasResults = scored.Count > 0;
        ShowQuiz = false;
        ShowResults = true;
    }

    private static int ScorePhone(Phone phone, string useCase)
    {
        var brand = phone.BrandName.ToLowerInvariant();
        var name = phone.Name.ToLowerInvariant();

        // Base score from price tier (mid-range gets bonus for value)
        int baseScore = phone.PricePHP switch
        {
            > 60000 => 80,
            > 30000 => 70,
            > 15000 => 60,
            _       => 50
        };

        int bonus = useCase switch
        {
            "camera" => brand switch
            {
                "apple"  => 25,
                "google" => 22,
                "vivo"   => 18,
                "xiaomi" when name.Contains("ultra") || name.Contains("pro") => 16,
                "samsung" when name.Contains("ultra") || name.Contains("pro") => 15,
                "oppo"   => 13,
                _        => 5
            },
            "gaming" => brand switch
            {
                "xiaomi"  when name.Contains("poco") => 25,
                "oneplus" => 22,
                "samsung" when name.Contains("s24") => 18,
                "xiaomi"  => 15,
                "realme"  when name.Contains("gt") => 14,
                "google"  => 12,
                _         => 5
            },
            "battery" => brand switch
            {
                "xiaomi"   when name.Contains("poco") => 22,
                "realme"   => 20,
                "motorola" => 18,
                "samsung"  when name.Contains("a") => 16,
                "oneplus"  => 14,
                "oppo"     => 13,
                _          => 8
            },
            "value" => phone.PricePHP switch
            {
                > 0 and <= 15000 => 25,
                > 15000 and <= 25000 => 20,
                > 25000 and <= 35000 => 15,
                > 35000 and <= 50000 => 10,
                _ => 5
            },
            _ => 0
        };

        return baseScore + bonus;
    }
}

public class QuizResult
{
    public Phone Phone { get; set; } = new();
    public int Score { get; set; }
    public bool IsBestMatch { get; set; }
    public int Rank { get; set; }
    public string RankLabel => IsBestMatch ? "🏆 Best Match" : $"#{Rank}";
    public Color BadgeColor => IsBestMatch ? Color.FromArgb("#F39C12") : Color.FromArgb("#919191");
}
