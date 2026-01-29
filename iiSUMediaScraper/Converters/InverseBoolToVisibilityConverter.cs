using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a boolean to Visibility with inverse logic.
/// True becomes Collapsed, False becomes Visible.
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to Visibility with inverse logic.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <returns>Visibility.Collapsed if true, Visibility.Visible if false.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return Visibility.Collapsed;
        }

        if (value is bool b)
        {
            return b ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    public string? EqualsValue { get; set; }
}
