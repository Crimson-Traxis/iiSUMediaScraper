using iiSUMediaScraper.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a SourceFlag to Visibility based on whether that source supports image fetching.
/// IGN, IGDB, and SteamGridDB return Visible; YouTube returns Collapsed.
/// </summary>
public class CanFetchImagesToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a SourceFlag to Visibility based on image support.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SourceFlag sourceFlag)
        {
            switch (sourceFlag)
            {
                case SourceFlag.Ign:
                case SourceFlag.Igdb:
                case SourceFlag.SteamGridDb:
                    return Visibility.Visible;
                case SourceFlag.Youtube:
                    return Visibility.Collapsed;
            }
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
}
