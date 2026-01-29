using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a value to Visibility based on whether it's null.
/// Null values become Collapsed, non-null values become Visible.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a value to Visibility.
    /// </summary>
    /// <param name="value">The value to check for null.</param>
    /// <returns>Visibility.Collapsed if null, Visibility.Visible otherwise.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    public string? EqualsValue { get; set; }
}
