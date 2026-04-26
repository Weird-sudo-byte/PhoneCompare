using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class CompareHistoryPage : ContentPage
{
    public CompareHistoryPage(CompareHistoryPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
