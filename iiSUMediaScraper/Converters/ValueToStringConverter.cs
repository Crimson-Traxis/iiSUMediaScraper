using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts any value to its string representation.
/// </summary>
public class ValueToStringConverter : IValueConverter
{
    /// <summary>
    /// Converts a value to its string representation.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value?.ToString() ?? "";
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
