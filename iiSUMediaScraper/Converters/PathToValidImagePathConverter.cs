using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace iiSUMediaScraper.Converters;

public class PathToValidImagePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            try
            {
                return new BitmapImage(new Uri(s));
            }
            catch
            {
                return null;
            }
        }
        else if (value is byte[] bytes)
        {
            BitmapImage image = new BitmapImage();

            MemoryStream stream = new MemoryStream(bytes);

            image.SetSource(stream.AsRandomAccessStream());

            return image;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    public string? EqualsValue { get; set; }
}
