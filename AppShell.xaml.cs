using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PhoneCompare.Pages;
using Font = Microsoft.Maui.Font;

namespace PhoneCompare
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));
            Routing.RegisterRoute("detail",   typeof(PhoneDetailPage));
            Routing.RegisterRoute("compare",  typeof(ComparisonPage));
            Routing.RegisterRoute("comparehistory", typeof(CompareHistoryPage));
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("seed",     typeof(SeedDataPage));
            Routing.RegisterRoute("onboarding", typeof(OnboardingPage));
            Routing.RegisterRoute("quiz", typeof(QuizPage));

            // Login is the default content — shown first to avoid flicker for returning users.
            // Only redirect to onboarding for first-time users.
            bool seen = Preferences.Get("onboarding_seen", false);
            if (!seen)
            {
                foreach (var item in Items)
                {
                    foreach (var section in item.Items)
                    {
                        foreach (var content in section.Items)
                        {
                            if (content.Route == "onboarding")
                            {
                                CurrentItem = item;
                                break;
                            }
                        }
                    }
                }
            }
        }
        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Font.SystemFontOfSize(18),
                ActionButtonFont = Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        public static async Task DisplayToastAsync(string message)
        {
            // Toast is currently not working in MCT on Windows
            if (OperatingSystem.IsWindows())
                return;

            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object? sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }
    }
}
