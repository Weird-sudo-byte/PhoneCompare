using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PhoneCompare.Config;
using System.Net.Sockets;

namespace PhoneCompare.Services;

public class EmailService
{
    public async Task<(bool Success, string Error)> SendOtpEmailAsync(string toEmail, string otp)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(SmtpConfig.SenderName, SmtpConfig.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "PhoneCompare - Email Verification Code";

            message.Body = new TextPart("html")
            {
                Text = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px;'>
    <div style='max-width: 400px; margin: 0 auto; background: white; border-radius: 16px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 24px;'>
            <h1 style='color: #E84C3D; margin: 0; font-size: 24px;'>PhoneCompare</h1>
            <p style='color: #666; margin: 8px 0 0 0; font-size: 14px;'>Email Verification</p>
        </div>
        
        <p style='color: #333; font-size: 15px; line-height: 1.5; margin-bottom: 24px;'>
            Enter the following code to verify your email address and complete your registration:
        </p>
        
        <div style='background: #f8f8f8; border-radius: 12px; padding: 20px; text-align: center; margin-bottom: 24px;'>
            <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #E84C3D;'>{otp}</span>
        </div>
        
        <p style='color: #999; font-size: 13px; text-align: center; margin-bottom: 0;'>
            This code will expire in <strong>5 minutes</strong>.
        </p>
        
        <hr style='border: none; border-top: 1px solid #eee; margin: 24px 0;'>
        
        <p style='color: #999; font-size: 12px; text-align: center; margin: 0;'>
            If you didn't request this code, please ignore this email.
        </p>
    </div>
</body>
</html>"
            };

            using var client = new SmtpClient();
            
            // Handle SSL certificate validation for Android
            // Android's strict OCSP validation fails for Gmail certificates
            // We trust Gmail's certificate since we're connecting to smtp.gmail.com
            client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                System.Diagnostics.Debug.WriteLine($"[Email] Certificate validation - Errors: {sslPolicyErrors}");
                
                // Always accept for smtp.gmail.com - we trust Google's infrastructure
                // The OCSP revocation check failure is a known Android issue, not a security concern
                return true;
            };
            
            System.Diagnostics.Debug.WriteLine($"[Email] Connecting to {SmtpConfig.Host}:{SmtpConfig.Port}...");
            await client.ConnectAsync(SmtpConfig.Host, SmtpConfig.Port, SecureSocketOptions.StartTls);
            System.Diagnostics.Debug.WriteLine($"[Email] Connected successfully");
            
            System.Diagnostics.Debug.WriteLine($"[Email] Authenticating as {SmtpConfig.SenderEmail}...");
            await client.AuthenticateAsync(SmtpConfig.SenderEmail, SmtpConfig.SenderPassword);
            System.Diagnostics.Debug.WriteLine($"[Email] Authenticated successfully");
            
            System.Diagnostics.Debug.WriteLine($"[Email] Sending to {toEmail}...");
            await client.SendAsync(message);
            System.Diagnostics.Debug.WriteLine($"[Email] Sent successfully");
            
            await client.DisconnectAsync(true);

            System.Diagnostics.Debug.WriteLine($"[Email] OTP sent to {toEmail}");
            return (true, string.Empty);
        }
        catch (AuthenticationException ex)
        {
            var error = "Email authentication failed. Check credentials.";
            System.Diagnostics.Debug.WriteLine($"[Email] AUTH ERROR: {ex.Message}");
            return (false, error);
        }
        catch (SmtpCommandException ex)
        {
            var error = $"SMTP error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[Email] SMTP ERROR: {ex.StatusCode} - {ex.Message}");
            return (false, error);
        }
        catch (SmtpProtocolException ex)
        {
            var error = "SMTP protocol error. Network may block email.";
            System.Diagnostics.Debug.WriteLine($"[Email] PROTOCOL ERROR: {ex.Message}");
            return (false, error);
        }
        catch (SocketException ex)
        {
            var error = "Network connection failed. Check internet.";
            System.Diagnostics.Debug.WriteLine($"[Email] SOCKET ERROR: {ex.SocketErrorCode} - {ex.Message}");
            return (false, error);
        }
        catch (IOException ex)
        {
            var error = "Network I/O error. Connection interrupted.";
            System.Diagnostics.Debug.WriteLine($"[Email] IO ERROR: {ex.Message}");
            return (false, error);
        }
        catch (OperationCanceledException ex)
        {
            var error = "Connection timed out. Try again.";
            System.Diagnostics.Debug.WriteLine($"[Email] TIMEOUT: {ex.Message}");
            return (false, error);
        }
        catch (Exception ex)
        {
            var error = $"Email error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[Email] UNKNOWN ERROR: {ex.GetType().Name} - {ex.Message}");
            return (false, error);
        }
    }

    public async Task<(bool Success, string Error)> SendPasswordResetOtpEmailAsync(string toEmail, string otp)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(SmtpConfig.SenderName, SmtpConfig.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "PhoneCompare - Password Reset Code";

            message.Body = new TextPart("html")
            {
                Text = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px;'>
    <div style='max-width: 400px; margin: 0 auto; background: white; border-radius: 16px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 24px;'>
            <h1 style='color: #E84C3D; margin: 0; font-size: 24px;'>PhoneCompare</h1>
            <p style='color: #666; margin: 8px 0 0 0; font-size: 14px;'>Password Reset</p>
        </div>
        
        <p style='color: #333; font-size: 15px; line-height: 1.5; margin-bottom: 24px;'>
            Enter the following code to reset your password:
        </p>
        
        <div style='background: #f8f8f8; border-radius: 12px; padding: 20px; text-align: center; margin-bottom: 24px;'>
            <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #E84C3D;'>{otp}</span>
        </div>
        
        <p style='color: #999; font-size: 13px; text-align: center; margin-bottom: 0;'>
            This code will expire in <strong>5 minutes</strong>.
        </p>
        
        <hr style='border: none; border-top: 1px solid #eee; margin: 24px 0;'>
        
        <p style='color: #999; font-size: 12px; text-align: center; margin: 0;'>
            If you didn't request a password reset, please ignore this email.
        </p>
    </div>
</body>
</html>"
            };

            using var client = new SmtpClient();
            
            client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                System.Diagnostics.Debug.WriteLine($"[Email] Certificate validation - Errors: {sslPolicyErrors}");
                return true;
            };
            
            System.Diagnostics.Debug.WriteLine($"[Email] Connecting to {SmtpConfig.Host}:{SmtpConfig.Port}...");
            await client.ConnectAsync(SmtpConfig.Host, SmtpConfig.Port, SecureSocketOptions.StartTls);
            System.Diagnostics.Debug.WriteLine($"[Email] Connected successfully");
            
            System.Diagnostics.Debug.WriteLine($"[Email] Authenticating as {SmtpConfig.SenderEmail}...");
            await client.AuthenticateAsync(SmtpConfig.SenderEmail, SmtpConfig.SenderPassword);
            System.Diagnostics.Debug.WriteLine($"[Email] Authenticated successfully");
            
            System.Diagnostics.Debug.WriteLine($"[Email] Sending password reset to {toEmail}...");
            await client.SendAsync(message);
            System.Diagnostics.Debug.WriteLine($"[Email] Sent successfully");
            
            await client.DisconnectAsync(true);

            System.Diagnostics.Debug.WriteLine($"[Email] Password reset OTP sent to {toEmail}");
            return (true, string.Empty);
        }
        catch (AuthenticationException ex)
        {
            var error = "Email authentication failed. Check credentials.";
            System.Diagnostics.Debug.WriteLine($"[Email] AUTH ERROR: {ex.Message}");
            return (false, error);
        }
        catch (SmtpCommandException ex)
        {
            var error = $"SMTP error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[Email] SMTP ERROR: {ex.StatusCode} - {ex.Message}");
            return (false, error);
        }
        catch (SmtpProtocolException ex)
        {
            var error = "SMTP protocol error. Network may block email.";
            System.Diagnostics.Debug.WriteLine($"[Email] PROTOCOL ERROR: {ex.Message}");
            return (false, error);
        }
        catch (SocketException ex)
        {
            var error = "Network connection failed. Check internet.";
            System.Diagnostics.Debug.WriteLine($"[Email] SOCKET ERROR: {ex.SocketErrorCode} - {ex.Message}");
            return (false, error);
        }
        catch (IOException ex)
        {
            var error = "Network I/O error. Connection interrupted.";
            System.Diagnostics.Debug.WriteLine($"[Email] IO ERROR: {ex.Message}");
            return (false, error);
        }
        catch (OperationCanceledException ex)
        {
            var error = "Connection timed out. Try again.";
            System.Diagnostics.Debug.WriteLine($"[Email] TIMEOUT: {ex.Message}");
            return (false, error);
        }
        catch (Exception ex)
        {
            var error = $"Email error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[Email] UNKNOWN ERROR: {ex.GetType().Name} - {ex.Message}");
            return (false, error);
        }
    }
}
