using System.Globalization;

namespace PhoneCompare.Converters;

public class PercentToWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percent)
        {
            // Return width based on percentage (max 200 for visual display)
            return Math.Max(10, percent * 200);
        }
        return 10;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
