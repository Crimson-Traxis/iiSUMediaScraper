using Microsoft.UI.Xaml.Data;
using System.IO;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a local file path to a data URI with HTML for displaying images in WebView2.
/// </summary>
public class PathToFileUriConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            try
            {
                // Read file and convert to base64 data URI
                var bytes = File.ReadAllBytes(path);
                var base64 = System.Convert.ToBase64String(bytes);
                var extension = Path.GetExtension(path).ToLower().TrimStart('.');
                var mimeType = extension switch
                {
                    "webp" => "image/webp",
                    "gif" => "image/gif",
                    "png" => "image/png",
                    "jpg" or "jpeg" => "image/jpeg",
                    _ => "image/webp"
                };

                // Create HTML with the image centered and filling the container
                var html = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        * {{ margin: 0; padding: 0; }}
        html, body {{ width: 100%; height: 100%; overflow: hidden; background: transparent; }}
        img {{ width: 100%; height: 100%; object-fit: cover; }}
    </style>
</head>
<body>
    <img src=""data:{mimeType};base64,{base64}"" />
</body>
</html>";

                // Return as data URI for WebView2
                var htmlBase64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html));
                return new Uri($"data:text/html;base64,{htmlBase64}");
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
