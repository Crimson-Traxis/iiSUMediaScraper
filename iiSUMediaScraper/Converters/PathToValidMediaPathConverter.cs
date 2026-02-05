using Microsoft.UI.Xaml.Data;
using Windows.Media.Core;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a file path string to a MediaSource for use with MediaPlayerElement.
/// </summary>
public class PathToValidMediaPathConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path))
        {
            try
            {
                Uri uri;

                if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
                {
                    uri = absoluteUri;
                }
                else
                {
                    uri = new Uri(path, UriKind.RelativeOrAbsolute);
                }

                return MediaSource.CreateFromUri(uri);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
