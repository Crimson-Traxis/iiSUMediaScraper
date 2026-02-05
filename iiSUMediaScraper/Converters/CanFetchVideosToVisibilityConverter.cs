using iiSUMediaScraper.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a SourceFlag to Visibility based on whether that source supports video fetching.
/// IGDB returns Visible; other sources return Collapsed.
/// </summary>
public class CanFetchVideosToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a SourceFlag to Visibility based on video support.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SourceFlag sourceFlag)
        {
            switch (sourceFlag)
            {
                case SourceFlag.Igdb:
                    return Visibility.Visible;
                case SourceFlag.SteamGridDb:
                case SourceFlag.Ign:
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
