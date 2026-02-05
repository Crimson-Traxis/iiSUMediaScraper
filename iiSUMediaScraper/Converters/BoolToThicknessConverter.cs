using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a boolean value to a Thickness based on configurable true/false values.
/// </summary>
public class BoolToThicknessConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to the configured Thickness value.
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
    /// Gets or sets the Thickness value returned when input is true.
    /// </summary>
    public Thickness TrueValue { get; set; }

    /// <summary>
    /// Gets or sets the Thickness value returned when input is false or null.
    /// </summary>
    public Thickness FalseValue { get; set; }
}
