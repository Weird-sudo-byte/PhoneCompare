namespace PhoneCompare.Models;

public class Favorite
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PhoneSlug { get; set; } = string.Empty;
    public string PhoneName { get; set; } = string.Empty;
    public string PhoneImageUrl { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
