namespace PhoneCompare.Pages;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage(OnboardingPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
        MainCarousel.IndicatorView = SlideIndicator;
    }
}
