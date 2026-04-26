using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class PhoneListPage : ContentPage
{
    public PhoneListPage(PhoneListPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
