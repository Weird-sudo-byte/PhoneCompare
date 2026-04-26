namespace PhoneCompare.Pages;

public partial class QuizPage : ContentPage
{
    public QuizPage(QuizPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
