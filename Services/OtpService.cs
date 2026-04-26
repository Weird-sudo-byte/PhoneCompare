namespace PhoneCompare.Services;

public class OtpService
{
    private readonly Dictionary<string, OtpData> _otpStore = new();
    private readonly Dictionary<string, RateLimitData> _rateLimits = new();
    
    private static readonly TimeSpan OtpExpiry = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(10);
    private const int MaxAttemptsPerWindow = 3;

    public (bool Success, string Message, string? Otp) GenerateOtp(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        
        if (!CheckRateLimit(normalizedEmail))
        {
            return (false, "Too many attempts. Please wait 10 minutes before requesting another code.", null);
        }

        var otp = Random.Shared.Next(100000, 999999).ToString();
        
        _otpStore[normalizedEmail] = new OtpData
        {
            Code = otp,
            ExpiresAt = DateTime.UtcNow.Add(OtpExpiry)
        };

        RecordAttempt(normalizedEmail);
        
        System.Diagnostics.Debug.WriteLine($"[OTP] Generated {otp} for {email}, expires at {_otpStore[normalizedEmail].ExpiresAt}");
        
        return (true, "OTP generated successfully", otp);
    }

    public (bool Success, string Message) ValidateOtp(string email, string otp)
    {
        var normalizedEmail = email.ToLowerInvariant();
        
        if (!_otpStore.TryGetValue(normalizedEmail, out var data))
        {
            return (false, "No verification code found. Please request a new code.");
        }

        if (DateTime.UtcNow > data.ExpiresAt)
        {
            _otpStore.Remove(normalizedEmail);
            return (false, "Verification code has expired. Please request a new code.");
        }

        if (data.Code != otp)
        {
            return (false, "Invalid verification code. Please try again.");
        }

        _otpStore.Remove(normalizedEmail);
        _rateLimits.Remove(normalizedEmail);
        
        System.Diagnostics.Debug.WriteLine($"[OTP] Validated successfully for {email}");
        
        return (true, "Email verified successfully");
    }

    public int GetRemainingAttempts(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        CleanupExpiredAttempts(normalizedEmail);
        
        if (!_rateLimits.TryGetValue(normalizedEmail, out var data))
            return MaxAttemptsPerWindow;
            
        return Math.Max(0, MaxAttemptsPerWindow - data.Attempts.Count);
    }

    private bool CheckRateLimit(string email)
    {
        CleanupExpiredAttempts(email);
        
        if (!_rateLimits.TryGetValue(email, out var data))
            return true;

        return data.Attempts.Count < MaxAttemptsPerWindow;
    }

    private void RecordAttempt(string email)
    {
        if (!_rateLimits.ContainsKey(email))
        {
            _rateLimits[email] = new RateLimitData();
        }
        
        _rateLimits[email].Attempts.Add(DateTime.UtcNow);
    }

    private void CleanupExpiredAttempts(string email)
    {
        if (!_rateLimits.TryGetValue(email, out var data))
            return;

        var cutoff = DateTime.UtcNow.Subtract(RateLimitWindow);
        data.Attempts.RemoveAll(t => t < cutoff);

        if (data.Attempts.Count == 0)
        {
            _rateLimits.Remove(email);
        }
    }

    private class OtpData
    {
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    private class RateLimitData
    {
        public List<DateTime> Attempts { get; set; } = new();
    }
}
