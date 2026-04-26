using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class SeedDataPage : ContentPage
{
    public SeedDataPage(SeedDataPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
