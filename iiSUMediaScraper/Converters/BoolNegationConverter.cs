using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a boolean value to its negation.
/// True becomes false, false (or null) becomes false.
/// </summary>
public class BoolNegationConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to its negation.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return false;
        }

        if (value is bool b && b)
        {
            return !b;
        }

        return false;
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
