using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class RegisterPageModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly EmailService _emailService;
    private readonly OtpService _otpService;

    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private bool _showOtpInput;
    [ObservableProperty] private string _otpCode = string.Empty;
    [ObservableProperty] private string _pendingEmail = string.Empty;
    [ObservableProperty] private string _pendingPassword = string.Empty;
    [ObservableProperty] private string _pendingName = string.Empty;
    [ObservableProperty] private string _otpStatusMessage = string.Empty;

    public RegisterPageModel(IAuthService auth, EmailService emailService, OtpService otpService)
    {
        _auth = auth;
        _emailService = emailService;
        _otpService = otpService;
    }

    [RelayCommand]
    private async Task Register()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "All fields are required.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters.";
            return;
        }

        // Check network connectivity
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            ErrorMessage = "No internet connection. Please check your network.";
            return;
        }

        IsBusy = true;
        try
        {
            var (success, message, otp) = _otpService.GenerateOtp(Email);
            if (!success)
            {
                ErrorMessage = message;
                return;
            }

            var (sent, emailError) = await _emailService.SendOtpEmailAsync(Email, otp!);
            if (!sent)
            {
                ErrorMessage = string.IsNullOrEmpty(emailError) 
                    ? "Failed to send verification email. Please try again."
                    : emailError;
                return;
            }

            PendingEmail = Email;
            PendingPassword = Password;
            PendingName = DisplayName;
            OtpStatusMessage = $"We sent a 6-digit code to {Email}";
            ShowOtpInput = true;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task VerifyOtp()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(OtpCode))
        {
            ErrorMessage = "Please enter the verification code.";
            return;
        }

        IsBusy = true;
        try
        {
            var (valid, validationMessage) = _otpService.ValidateOtp(PendingEmail, OtpCode);
            if (!valid)
            {
                ErrorMessage = validationMessage;
                return;
            }

            var (success, message, _) = await _auth.RegisterAsync(PendingEmail, PendingPassword, PendingName);
            if (success)
            {
                await Shell.Current.GoToAsync("//phones");
            }
            else
            {
                ErrorMessage = message;
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ResendOtp()
    {
        ErrorMessage = string.Empty;

        // Check network connectivity
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            ErrorMessage = "No internet connection. Please check your network.";
            return;
        }

        IsBusy = true;

        try
        {
            var (success, message, otp) = _otpService.GenerateOtp(PendingEmail);
            if (!success)
            {
                ErrorMessage = message;
                return;
            }

            var (sent, emailError) = await _emailService.SendOtpEmailAsync(PendingEmail, otp!);
            if (!sent)
            {
                ErrorMessage = string.IsNullOrEmpty(emailError)
                    ? "Failed to send verification email. Please try again."
                    : emailError;
                return;
            }

            OtpStatusMessage = "A new code has been sent to your email.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void BackToForm()
    {
        ShowOtpInput = false;
        OtpCode = string.Empty;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private Task GoBack() => Shell.Current.GoToAsync("..");
}
