namespace PhoneCompare.Models;

public class RetailerPrice
{
    public string Retailer { get; set; } = string.Empty;
    public string LogoEmoji { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool InStock { get; set; } = true;
    public bool IsBestPrice { get; set; }

    public string PriceDisplay => Price > 0 ? $"₱{Price:N0}" : "Check Store";
    public string StockStatus => InStock ? "In Stock" : "Out of Stock";
    public string StockColor => InStock ? "#4CAF50" : "#F44336";
}
