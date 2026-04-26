using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public LoginPageModel(IAuthService auth) => _auth = auth;

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your email and password.";
            return;
        }

        IsBusy = true;
        try
        {
            var (success, message, _) = await _auth.LoginAsync(Email, Password);
            if (success)
                await Shell.Current.GoToAsync("//phones");
            else
                ErrorMessage = message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private Task GoToRegister() => Shell.Current.GoToAsync("register");

    [RelayCommand]
    private Task GoToForgotPassword() => Shell.Current.GoToAsync("forgotpassword");
}
