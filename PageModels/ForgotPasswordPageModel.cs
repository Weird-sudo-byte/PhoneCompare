using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneCompare.Services;

namespace PhoneCompare.PageModels;

public partial class ForgotPasswordPageModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly EmailService _emailService;
    private readonly OtpService _otpService;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _otpCode = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private bool _showEmailStep = true;
    [ObservableProperty] private bool _showOtpStep;
    [ObservableProperty] private bool _showPasswordStep;

    private string _pendingEmail = string.Empty;
    private string _pendingOobCode = string.Empty;

    public ForgotPasswordPageModel(IAuthService auth, EmailService emailService, OtpService otpService)
    {
        _auth = auth;
        _emailService = emailService;
        _otpService = otpService;
    }

    [RelayCommand]
    private async Task SendOtp()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email address.";
            return;
        }

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

            var (sent, emailError) = await _emailService.SendPasswordResetOtpEmailAsync(Email, otp!);
            if (!sent)
            {
                ErrorMessage = string.IsNullOrEmpty(emailError) 
                    ? "Failed to send verification email. Please try again."
                    : emailError;
                return;
            }

            _pendingEmail = Email;
            StatusMessage = $"We sent a 6-digit code to {Email}";
            ShowEmailStep = false;
            ShowOtpStep = true;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task VerifyOtp()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(OtpCode))
        {
            ErrorMessage = "Please enter the verification code.";
            return;
        }

        var (valid, validationMessage) = _otpService.ValidateOtp(_pendingEmail, OtpCode);
        if (!valid)
        {
            ErrorMessage = validationMessage;
            return;
        }

        // OTP verified - now send Firebase password reset email
        IsBusy = true;
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPassword] Sending Firebase reset email to {_pendingEmail}");
            var (success, message) = await _auth.ResetPasswordAsync(_pendingEmail, string.Empty);
            System.Diagnostics.Debug.WriteLine($"[ForgotPassword] Result: success={success}, message={message}");
            
            if (success)
            {
                await Shell.Current.DisplayAlert(
                    "Check Your Email", 
                    "Identity verified! A password reset link has been sent to your email. Click the link to set your new password.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ForgotPassword] ERROR: {message}");
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPassword] EXCEPTION: {ex.Message}");
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ResetPassword()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "Please enter a new password.";
            return;
        }

        if (NewPassword.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        if (string.IsNullOrEmpty(_pendingOobCode))
        {
            ErrorMessage = "Session expired. Please start over.";
            return;
        }

        IsBusy = true;
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPassword] Confirming password reset");
            var (success, message) = await _auth.ConfirmPasswordResetAsync(_pendingOobCode, NewPassword);
            System.Diagnostics.Debug.WriteLine($"[ForgotPassword] Reset result: success={success}, message={message}");

            if (success)
            {
                await Shell.Current.DisplayAlert(
                    "Success!", 
                    "Your password has been reset successfully. You can now login with your new password.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPassword] Reset EXCEPTION: {ex.Message}");
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ResendOtp()
    {
        ErrorMessage = string.Empty;

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            ErrorMessage = "No internet connection. Please check your network.";
            return;
        }

        IsBusy = true;
        try
        {
            var (success, message, otp) = _otpService.GenerateOtp(_pendingEmail);
            if (!success)
            {
                ErrorMessage = message;
                return;
            }

            var (sent, emailError) = await _emailService.SendPasswordResetOtpEmailAsync(_pendingEmail, otp!);
            if (!sent)
            {
                ErrorMessage = string.IsNullOrEmpty(emailError)
                    ? "Failed to send verification email. Please try again."
                    : emailError;
                return;
            }

            StatusMessage = "A new code has been sent to your email.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void BackToEmail()
    {
        ShowOtpStep = false;
        ShowPasswordStep = false;
        ShowEmailStep = true;
        OtpCode = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private Task GoBack() => Shell.Current.GoToAsync("..");
}
