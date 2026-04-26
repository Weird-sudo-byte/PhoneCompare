using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class AccountPage : ContentPage
{
    public AccountPage(AccountPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
