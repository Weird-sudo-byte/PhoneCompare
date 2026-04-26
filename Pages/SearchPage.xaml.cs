using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class SearchPage : ContentPage
{
    public SearchPage(SearchPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
