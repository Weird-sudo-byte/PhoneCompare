using PhoneCompare.Models;

namespace PhoneCompare.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, AppUser? User)> LoginAsync(string email, string password);
    Task<(bool Success, string Message, AppUser? User)> RegisterAsync(string email, string password, string displayName);
    Task<(bool Success, string Message)> ResetPasswordAsync(string email, string newPassword);
    Task<(bool Success, string Message, string? OobCode)> GetPasswordResetOobCodeAsync(string email);
    Task<(bool Success, string Message)> ConfirmPasswordResetAsync(string oobCode, string newPassword);
    Task LogoutAsync();
    AppUser? GetCurrentUser();
    bool IsLoggedIn { get; }
}
