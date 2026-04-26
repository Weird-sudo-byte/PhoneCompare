using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class ComparisonPage : ContentPage
{
    public ComparisonPage(ComparisonPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
