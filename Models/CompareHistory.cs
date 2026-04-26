namespace PhoneCompare.Models;

public class CompareHistory
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    
    public string Phone1Slug { get; set; } = string.Empty;
    public string Phone1Name { get; set; } = string.Empty;
    public string Phone1ImageUrl { get; set; } = string.Empty;
    
    public string Phone2Slug { get; set; } = string.Empty;
    public string Phone2Name { get; set; } = string.Empty;
    public string Phone2ImageUrl { get; set; } = string.Empty;
    
    public DateTime ComparedAt { get; set; } = DateTime.UtcNow;
}
