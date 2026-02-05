using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a boolean value to an integer based on configurable true/false values.
/// </summary>
public class BoolToIntegerConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to the configured integer value.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return FalseValue;
        }

        if (value is bool b && b)
        {
            return TrueValue;
        }

        return FalseValue;
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets or sets the integer value returned when input is true.
    /// </summary>
    public int TrueValue { get; set; }

    /// <summary>
    /// Gets or sets the integer value returned when input is false or null.
    /// </summary>
    public int FalseValue { get; set; }
}
