using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class PhoneDetailPage : ContentPage
{
    public PhoneDetailPage(PhoneDetailPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
