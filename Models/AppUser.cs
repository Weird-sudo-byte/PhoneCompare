namespace PhoneCompare.Models;

public class AppUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
