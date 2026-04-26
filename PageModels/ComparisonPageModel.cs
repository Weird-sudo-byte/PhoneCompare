using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Models;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

[QueryProperty(nameof(Slug1), "slug1")]
[QueryProperty(nameof(Name1), "name1")]
[QueryProperty(nameof(Slug2), "slug2")]
[QueryProperty(nameof(Name2), "name2")]
public partial class ComparisonPageModel : ObservableObject
{
    private readonly IPhoneApiService _phoneApi;
    private readonly CompareHistoryService _historyService;
    private readonly IAuthService _auth;

    [ObservableProperty] private string _slug1 = string.Empty;
    [ObservableProperty] private string _name1 = string.Empty;
    [ObservableProperty] private string _slug2 = string.Empty;
    [ObservableProperty] private string _name2 = string.Empty;
    [ObservableProperty] private PhoneDetail? _phone1;
    [ObservableProperty] private PhoneDetail? _phone2;
    [ObservableProperty] private List<ComparisonRow> _rows = [];
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _winnerSummary = string.Empty;
    [ObservableProperty] private int _phone1Wins;
    [ObservableProperty] private int _phone2Wins;
    [ObservableProperty] private List<SpecBar> _keySpecs = [];
    [ObservableProperty] private string _overallWinner = string.Empty;
    [ObservableProperty] private bool _isPhone1Overall;
    [ObservableProperty] private bool _isPhone2Overall;
    [ObservableProperty] private bool _isTie;

    public bool HasBothPhones => Phone1 != null && Phone2 != null;

    public ComparisonPageModel(IPhoneApiService phoneApi, CompareHistoryService historyService, IAuthService auth)
    {
        _phoneApi = phoneApi;
        _historyService = historyService;
        _auth = auth;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        if (string.IsNullOrEmpty(Slug1) || string.IsNullOrEmpty(Slug2)) return;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task Share()
    {
        if (Phone1 == null || Phone2 == null) return;
        
        var deepLink = $"phonecompare://compare/{Slug1}/{Slug2}";
        var keySpecsText = string.Join("\n", KeySpecs.Select(s => 
            $"  {s.Icon} {s.Label}: {s.Display1} vs {s.Display2}" + 
            (s.Winner == 1 ? " ✓" : s.Winner == 2 ? " ✗" : "")));
        
        var shareText = $"📱 Phone Comparison\n\n" +
                        $"🆚 {Phone1.Name} vs {Phone2.Name}\n\n" +
                        $"🏆 {WinnerSummary}\n\n" +
                        $"Key Specs:\n{keySpecsText}\n\n" +
                        $"Compare on PhoneCompare!\n{deepLink}";
        
        await Microsoft.Maui.ApplicationModel.DataTransfer.Share.RequestAsync(new ShareTextRequest
        {
            Text = shareText,
            Title = "Share Comparison"
        });
    }

    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var t1 = _phoneApi.GetPhoneDetailAsync(Slug1);
            var t2 = _phoneApi.GetPhoneDetailAsync(Slug2);
            await Task.WhenAll(t1, t2);

            Phone1 = t1.Result;
            Phone2 = t2.Result;
            OnPropertyChanged(nameof(HasBothPhones));

            if (Phone1 != null && Phone2 != null)
            {
                Rows = BuildRows(Phone1, Phone2);
                Phone1Wins = Rows.Count(r => r.Winner == 1);
                Phone2Wins = Rows.Count(r => r.Winner == 2);

                // Set overall winner state
                IsPhone1Overall = Phone1Wins > Phone2Wins;
                IsPhone2Overall = Phone2Wins > Phone1Wins;
                IsTie = Phone1Wins == Phone2Wins;

                if (IsPhone1Overall)
                {
                    WinnerSummary = $"{Phone1.Name} wins ({Phone1Wins} vs {Phone2Wins})";
                    OverallWinner = Phone1.Name;
                }
                else if (IsPhone2Overall)
                {
                    WinnerSummary = $"{Phone2.Name} wins ({Phone2Wins} vs {Phone1Wins})";
                    OverallWinner = Phone2.Name;
                }
                else
                {
                    WinnerSummary = $"It's a tie! ({Phone1Wins} - {Phone2Wins})";
                    OverallWinner = "Tie";
                }

                // Build key specs for visual comparison
                KeySpecs = BuildKeySpecs(Phone1, Phone2);

                // Save to compare history
                await SaveToHistoryAsync();
            }
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task SaveToHistoryAsync()
    {
        var user = _auth.GetCurrentUser();
        if (user == null || Phone1 == null || Phone2 == null) return;

        try
        {
            var history = new CompareHistory
            {
                UserId = user.Id,
                Phone1Slug = Slug1,
                Phone1Name = Phone1.Name,
                Phone1ImageUrl = Phone1.ImageUrl ?? string.Empty,
                Phone2Slug = Slug2,
                Phone2Name = Phone2.Name,
                Phone2ImageUrl = Phone2.ImageUrl ?? string.Empty,
                ComparedAt = DateTime.UtcNow
            };

            await _historyService.AddHistoryAsync(history, user.IdToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CompareHistory] Save failed: {ex.Message}");
        }
    }

    // Keys where lower is better
    private static readonly HashSet<string> LowerIsBetter = new(StringComparer.OrdinalIgnoreCase)
    { "weight", "thickness", "price" };

    private static List<ComparisonRow> BuildRows(PhoneDetail p1, PhoneDetail p2)
    {
        var rows = new List<ComparisonRow>();
        var allGroups = p1.Specifications.Select(g => g.Title)
            .Union(p2.Specifications.Select(g => g.Title)).ToList();

        foreach (var groupTitle in allGroups)
        {
            rows.Add(new ComparisonRow { Label = groupTitle, Val1 = "", Val2 = "", IsHeader = true });

            var g1 = p1.Specifications.FirstOrDefault(g => g.Title == groupTitle);
            var g2 = p2.Specifications.FirstOrDefault(g => g.Title == groupTitle);

            var allKeys = (g1?.Specs.Select(s => s.Key) ?? [])
                .Union(g2?.Specs.Select(s => s.Key) ?? []).ToList();

            foreach (var key in allKeys)
            {
                var v1 = g1?.Specs.FirstOrDefault(s => s.Key == key)?.Value ?? "—";
                var v2 = g2?.Specs.FirstOrDefault(s => s.Key == key)?.Value ?? "—";
                var row = new ComparisonRow { Label = key, Val1 = v1, Val2 = v2, IsHeader = false };

                if (v1 != v2)
                {
                    row.IsDiff = true;
                    var n1 = ExtractNumber(v1);
                    var n2 = ExtractNumber(v2);

                    if (n1.HasValue && n2.HasValue && n1.Value != n2.Value)
                    {
                        bool lowerBetter = LowerIsBetter.Contains(key);
                        row.Winner = lowerBetter
                            ? (n1.Value < n2.Value ? 1 : 2)
                            : (n1.Value > n2.Value ? 1 : 2);
                    }
                }

                rows.Add(row);
            }
        }

        return rows;
    }

    private static double? ExtractNumber(string text)
    {
        // Match patterns like "5000 mAh", "6.8 inches", "120Hz", "200MP", "45W", "12GB"
        var match = Regex.Match(text, @"(\d+\.?\d*)");
        return match.Success && double.TryParse(match.Groups[1].Value, out var val) ? val : null;
    }

    private static List<SpecBar> BuildKeySpecs(PhoneDetail p1, PhoneDetail p2)
    {
        var specs = new List<SpecBar>();

        // Battery (mAh)
        var bat1 = ExtractNumber(p1.Battery) ?? 0;
        var bat2 = ExtractNumber(p2.Battery) ?? 0;
        var batMax = Math.Max(bat1, bat2);
        if (batMax > 0)
        {
            specs.Add(new SpecBar
            {
                Label = "Battery",
                Icon = "🔋",
                Value1 = bat1,
                Value2 = bat2,
                Display1 = $"{bat1:0} mAh",
                Display2 = $"{bat2:0} mAh",
                Percent1 = bat1 / batMax,
                Percent2 = bat2 / batMax,
                Winner = bat1 > bat2 ? 1 : bat2 > bat1 ? 2 : 0
            });
        }

        // RAM (GB)
        var ram1 = ExtractNumber(p1.Ram) ?? 0;
        var ram2 = ExtractNumber(p2.Ram) ?? 0;
        var ramMax = Math.Max(ram1, ram2);
        if (ramMax > 0)
        {
            specs.Add(new SpecBar
            {
                Label = "RAM",
                Icon = "💾",
                Value1 = ram1,
                Value2 = ram2,
                Display1 = $"{ram1:0} GB",
                Display2 = $"{ram2:0} GB",
                Percent1 = ram1 / ramMax,
                Percent2 = ram2 / ramMax,
                Winner = ram1 > ram2 ? 1 : ram2 > ram1 ? 2 : 0
            });
        }

        // Display (inches)
        var disp1 = ExtractNumber(p1.Display) ?? 0;
        var disp2 = ExtractNumber(p2.Display) ?? 0;
        var dispMax = Math.Max(disp1, disp2);
        if (dispMax > 0)
        {
            specs.Add(new SpecBar
            {
                Label = "Screen",
                Icon = "📱",
                Value1 = disp1,
                Value2 = disp2,
                Display1 = $"{disp1:0.0}\"",
                Display2 = $"{disp2:0.0}\"",
                Percent1 = disp1 / dispMax,
                Percent2 = disp2 / dispMax,
                Winner = disp1 > disp2 ? 1 : disp2 > disp1 ? 2 : 0
            });
        }

        // Camera (MP)
        var cam1 = ExtractNumber(p1.MainCamera) ?? 0;
        var cam2 = ExtractNumber(p2.MainCamera) ?? 0;
        var camMax = Math.Max(cam1, cam2);
        if (camMax > 0)
        {
            specs.Add(new SpecBar
            {
                Label = "Camera",
                Icon = "📷",
                Value1 = cam1,
                Value2 = cam2,
                Display1 = $"{cam1:0} MP",
                Display2 = $"{cam2:0} MP",
                Percent1 = cam1 / camMax,
                Percent2 = cam2 / camMax,
                Winner = cam1 > cam2 ? 1 : cam2 > cam1 ? 2 : 0
            });
        }

        return specs;
    }
}

public class SpecBar
{
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double Value1 { get; set; }
    public double Value2 { get; set; }
    public string Display1 { get; set; } = string.Empty;
    public string Display2 { get; set; } = string.Empty;
    public double Percent1 { get; set; }
    public double Percent2 { get; set; }
    public int Winner { get; set; } // 0=tie, 1=phone1, 2=phone2

    public GridLength Width1 => new(Math.Max(0.1, Percent1), GridUnitType.Star);
    public GridLength Width2 => new(Math.Max(0.1, Percent2), GridUnitType.Star);
    public GridLength Remainder1 => new(1 - Percent1, GridUnitType.Star);
    public GridLength Remainder2 => new(1 - Percent2, GridUnitType.Star);

    public Color Bar1Color => Winner == 1 ? Color.FromArgb("#4CAF50") : Color.FromArgb("#2196F3");
    public Color Bar2Color => Winner == 2 ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FF9800");

    public bool IsPhone1Winner => Winner == 1;
    public bool IsPhone2Winner => Winner == 2;
}

public class ComparisonRow
{
    public string Label { get; set; } = string.Empty;
    public string Val1  { get; set; } = string.Empty;
    public string Val2  { get; set; } = string.Empty;
    public bool IsHeader { get; set; }
    public bool IsDiff   { get; set; }
    public int  Winner   { get; set; } // 0=tie/same, 1=phone1 better, 2=phone2 better

    public bool IsPhone1Winner => Winner == 1;
    public bool IsPhone2Winner => Winner == 2;

    private static bool IsDarkMode => Application.Current?.RequestedTheme == AppTheme.Dark;
    public Color Val1Background => Winner == 1 ? (IsDarkMode ? Color.FromArgb("#14532D") : Color.FromArgb("#D4EDDA")) : Colors.Transparent;
    public Color Val2Background => Winner == 2 ? (IsDarkMode ? Color.FromArgb("#14532D") : Color.FromArgb("#D4EDDA")) : Colors.Transparent;
}
