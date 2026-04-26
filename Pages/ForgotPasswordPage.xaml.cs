using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage(ForgotPasswordPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
