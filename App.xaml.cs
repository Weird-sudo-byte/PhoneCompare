using Microsoft.Extensions.DependencyInjection;

namespace PhoneCompare
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);
            
            if (uri.Scheme != "phonecompare") return;
            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var path = uri.Host + uri.AbsolutePath;
                    
                    // phonecompare://phone/{slug}
                    if (path.StartsWith("phone/"))
                    {
                        var slug = path.Replace("phone/", "").Trim('/');
                        if (!string.IsNullOrEmpty(slug))
                        {
                            await Shell.Current.GoToAsync($"//phones/detail?slug={slug}&name={Uri.EscapeDataString(slug)}");
                        }
                    }
                    // phonecompare://compare/{slug1}/{slug2}
                    else if (path.StartsWith("compare/"))
                    {
                        var parts = path.Replace("compare/", "").Trim('/').Split('/');
                        if (parts.Length >= 2)
                        {
                            var slug1 = parts[0];
                            var slug2 = parts[1];
                            await Shell.Current.GoToAsync($"comparison?slug1={slug1}&name1={slug1}&slug2={slug2}&name2={slug2}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DeepLink] Error: {ex.Message}");
                }
            });
        }
    }
}