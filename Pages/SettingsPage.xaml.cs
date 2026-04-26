using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
