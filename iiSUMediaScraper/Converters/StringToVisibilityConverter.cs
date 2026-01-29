using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a string to Visibility based on whether it's null or whitespace.
/// Non-empty strings become Visible, null or empty strings become Collapsed.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a string to Visibility.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <returns>Visibility.Visible if string has content, Visibility.Collapsed otherwise.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
