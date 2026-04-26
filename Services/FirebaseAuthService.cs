using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PhoneCompare.Config;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

public class FirebaseAuthService : IAuthService
{
    private readonly HttpClient _http;
    private AppUser? _currentUser;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public FirebaseAuthService(HttpClient http) => _http = http;

    public bool IsLoggedIn => _currentUser != null || !string.IsNullOrEmpty(Preferences.Get("idToken", null));

    public AppUser? GetCurrentUser()
    {
        if (_currentUser != null) return _currentUser;

        var token = Preferences.Get("idToken", string.Empty);
        var uid   = Preferences.Get("uid",     string.Empty);
        var email = Preferences.Get("email",   string.Empty);
        var name  = Preferences.Get("name",    string.Empty);

        if (string.IsNullOrEmpty(uid)) return null;

        _currentUser = new AppUser 
        { 
            Id = uid, 
            Email = email, 
            DisplayName = name, 
            IdToken = token,
            IsAdmin = FirebaseConfig.IsAdmin(email)
        };
        return _currentUser;
    }

    public async Task<(bool Success, string Message, AppUser? User)> LoginAsync(string email, string password)
    {
        try
        {
            var url = $"{FirebaseConfig.AuthBaseUrl}/accounts:signInWithPassword?key={FirebaseConfig.ApiKey}";
            var response = await _http.PostAsJsonAsync(url, new { email, password, returnSecureToken = true });
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var err = JsonSerializer.Deserialize<FirebaseErrorResponse>(json, JsonOpts);
                return (false, FriendlyError(err?.Error?.Message), null);
            }

            var data = JsonSerializer.Deserialize<FirebaseAuthResponse>(json, JsonOpts);
            var user = BuildUser(data!, email);
            SavePrefs(user, data!.IdToken!);
            return (true, "Login successful", user);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message, AppUser? User)> RegisterAsync(string email, string password, string displayName)
    {
        try
        {
            var url = $"{FirebaseConfig.AuthBaseUrl}/accounts:signUp?key={FirebaseConfig.ApiKey}";
            var response = await _http.PostAsJsonAsync(url, new { email, password, returnSecureToken = true });
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var err = JsonSerializer.Deserialize<FirebaseErrorResponse>(json, JsonOpts);
                return (false, FriendlyError(err?.Error?.Message), null);
            }

            var data = JsonSerializer.Deserialize<FirebaseAuthResponse>(json, JsonOpts);
            var user = BuildUser(data!, email, displayName);
            SavePrefs(user, data!.IdToken!);
            return (true, "Registration successful", user);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string newPassword)
    {
        try
        {
            var url = $"{FirebaseConfig.AuthBaseUrl}/accounts:sendOobCode?key={FirebaseConfig.ApiKey}";
            var payload = new { requestType = "PASSWORD_RESET", email };
            
            System.Diagnostics.Debug.WriteLine($"[Auth] Sending password reset email to: {email}");
            
            var response = await _http.PostAsJsonAsync(url, payload);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"[Auth] Reset response status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[Auth] Reset response body: {responseBody}");
            
            if (!response.IsSuccessStatusCode)
            {
                var err = JsonSerializer.Deserialize<FirebaseErrorResponse>(responseBody, JsonOpts);
                var errorMsg = FriendlyError(err?.Error?.Message);
                System.Diagnostics.Debug.WriteLine($"[Auth] Reset ERROR: {errorMsg}");
                return (false, errorMsg);
            }

            System.Diagnostics.Debug.WriteLine($"[Auth] Password reset email sent successfully!");
            return (true, "Password reset email sent successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Auth] Reset EXCEPTION: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message, string? OobCode)> GetPasswordResetOobCodeAsync(string email)
    {
        try
        {
            var url = $"{FirebaseConfig.AuthBaseUrl}/accounts:sendOobCode?key={FirebaseConfig.ApiKey}";
            var payload = new { requestType = "PASSWORD_RESET", email };
            
            System.Diagnostics.Debug.WriteLine($"[Auth] Getting oobCode for: {email}");
            
            var response = await _http.PostAsJsonAsync(url, payload);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"[Auth] OobCode response: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[Auth] OobCode body: {responseBody}");
            
            if (!response.IsSuccessStatusCode)
            {
                var err = JsonSerializer.Deserialize<FirebaseErrorResponse>(responseBody, JsonOpts);
                return (false, FriendlyError(err?.Error?.Message), null);
            }

            var result = JsonSerializer.Deserialize<OobCodeResponse>(responseBody, JsonOpts);
            if (string.IsNullOrEmpty(result?.OobCode))
            {
                return (false, "Failed to get reset code from server", null);
            }

            System.Diagnostics.Debug.WriteLine($"[Auth] Got oobCode: {result.OobCode.Substring(0, 10)}...");
            return (true, "Reset code obtained", result.OobCode);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Auth] GetOobCode EXCEPTION: {ex.Message}");
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message)> ConfirmPasswordResetAsync(string oobCode, string newPassword)
    {
        try
        {
            var url = $"{FirebaseConfig.AuthBaseUrl}/accounts:resetPassword?key={FirebaseConfig.ApiKey}";
            var payload = new { oobCode, newPassword };
            
            System.Diagnostics.Debug.WriteLine($"[Auth] Confirming password reset");
            
            var response = await _http.PostAsJsonAsync(url, payload);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"[Auth] Confirm reset response: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[Auth] Confirm reset body: {responseBody}");
            
            if (!response.IsSuccessStatusCode)
            {
                var err = JsonSerializer.Deserialize<FirebaseErrorResponse>(responseBody, JsonOpts);
                return (false, FriendlyError(err?.Error?.Message));
            }

            System.Diagnostics.Debug.WriteLine($"[Auth] Password reset successful!");
            return (true, "Password reset successful! You can now login with your new password.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Auth] ConfirmReset EXCEPTION: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        Preferences.Remove("idToken");
        Preferences.Remove("uid");
        Preferences.Remove("email");
        Preferences.Remove("name");
        return Task.CompletedTask;
    }

    private AppUser BuildUser(FirebaseAuthResponse data, string email, string name = "")
    {
        _currentUser = new AppUser
        {
            Id = data.LocalId ?? string.Empty,
            Email = email,
            DisplayName = string.IsNullOrEmpty(name) ? email.Split('@')[0] : name,
            IdToken = data.IdToken ?? string.Empty,
            IsAdmin = FirebaseConfig.IsAdmin(email)
        };
        return _currentUser;
    }

    private void SavePrefs(AppUser user, string token)
    {
        Preferences.Set("idToken", token);
        Preferences.Set("uid",     user.Id);
        Preferences.Set("email",   user.Email);
        Preferences.Set("name",    user.DisplayName);
    }

    private static string FriendlyError(string? code) => code switch
    {
        "EMAIL_EXISTS"               => "This email is already registered.",
        "EMAIL_NOT_FOUND"            => "Invalid email or password.",
        "INVALID_PASSWORD"           => "Invalid email or password.",
        "INVALID_EMAIL"              => "Please enter a valid email address.",
        "WEAK_PASSWORD : Password should be at least 6 characters" => "Password must be at least 6 characters.",
        "TOO_MANY_ATTEMPTS_TRY_LATER"=> "Too many attempts. Please try again later.",
        "USER_DISABLED"              => "This account has been disabled.",
        _ => $"Authentication error: {code}"
    };

    private class FirebaseAuthResponse
    {
        public string? IdToken  { get; set; }
        public string? LocalId  { get; set; }
        public string? Email    { get; set; }
    }

    private class OobCodeResponse
    {
        public string? OobCode { get; set; }
        public string? Email   { get; set; }
    }

    private class FirebaseErrorResponse
    {
        public ErrorDetail? Error { get; set; }
        private class ErrorDetail2 { public string? Message { get; set; } }
    }

    private class FirebaseErrorResponse2
    {
        public ErrorDetail? Error { get; set; }
    }

    private class ErrorDetail
    {
        public string? Message { get; set; }
    }
}
