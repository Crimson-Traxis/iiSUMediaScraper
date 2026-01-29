using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

public class BoolToIntegerConverter : IValueConverter
{
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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    public int TrueValue { get; set; }

    public int FalseValue { get; set; }
}
