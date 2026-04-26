using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PhoneCompare.PageModels;
using PhoneCompare.Pages;
using PhoneCompare.Services;
using Syncfusion.Maui.Toolkit.Hosting;

namespace PhoneCompare
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            // HTTP
            builder.Services.AddHttpClient();

            // Services
            builder.Services.AddSingleton<PhoneDataCache>();
            builder.Services.AddSingleton<GsmArenaScraperService>();
            builder.Services.AddSingleton<FirestorePhoneService>();
            builder.Services.AddSingleton<PhoneSyncService>();
            builder.Services.AddSingleton<PhoneDataSeeder>();
            builder.Services.AddSingleton<IPhoneApiService, HybridPhoneService>();
            builder.Services.AddSingleton<IAuthService, FirebaseAuthService>();
            builder.Services.AddSingleton<LocalFavoritesService>();
            builder.Services.AddSingleton<IFavoritesService, FavoritesService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<CompareStateService>();
            builder.Services.AddSingleton<EmailService>();
            builder.Services.AddSingleton<OtpService>();
            builder.Services.AddSingleton<CompareHistoryService>();
            builder.Services.AddSingleton<RecentActivityService>();

            // PageModels
            builder.Services.AddSingleton<LoginPageModel>();
            builder.Services.AddSingleton<RegisterPageModel>();
            builder.Services.AddTransient<ForgotPasswordPageModel>();
            builder.Services.AddSingleton<PhoneListPageModel>();
            builder.Services.AddSingleton<SearchPageModel>();
            builder.Services.AddSingleton<FavoritesPageModel>();
            builder.Services.AddSingleton<AccountPageModel>();
            builder.Services.AddTransient<SeedDataPageModel>();
            builder.Services.AddTransient<PhoneDetailPageModel>();
            builder.Services.AddTransient<ComparisonPageModel>();
            builder.Services.AddTransient<CompareHistoryPageModel>();
            builder.Services.AddTransient<SettingsPageModel>();
            builder.Services.AddTransient<OnboardingPageModel>();
            builder.Services.AddTransient<QuizPageModel>();

            // Pages
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<RegisterPage>();
            builder.Services.AddTransient<ForgotPasswordPage>();
            builder.Services.AddSingleton<PhoneListPage>();
            builder.Services.AddSingleton<SearchPage>();
            builder.Services.AddSingleton<FavoritesPage>();
            builder.Services.AddSingleton<AccountPage>();
            builder.Services.AddTransient<SeedDataPage>();
            builder.Services.AddTransient<PhoneDetailPage>();
            builder.Services.AddTransient<ComparisonPage>();
            builder.Services.AddTransient<CompareHistoryPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<QuizPage>();

            return builder.Build();
        }
    }
}
