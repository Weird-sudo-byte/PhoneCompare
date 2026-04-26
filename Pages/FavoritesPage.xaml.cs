using PhoneCompare.PageModels;

namespace PhoneCompare.Pages;

public partial class FavoritesPage : ContentPage
{
    public FavoritesPage(FavoritesPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
