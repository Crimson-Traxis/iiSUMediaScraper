using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts an integer to Visibility based on whether it is less than a threshold.
/// </summary>
public class LessThanIntegerToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts an integer to Visibility.Visible if less than threshold, Collapsed otherwise.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return Visibility.Collapsed;
        }

        if (value is int i)
        {
            return i < LessThan ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets or sets the threshold value for comparison.
    /// </summary>
    public int LessThan { get; set; } = 1;
}
