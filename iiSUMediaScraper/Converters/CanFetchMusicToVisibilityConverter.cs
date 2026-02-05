using iiSUMediaScraper.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a SourceFlag to Visibility based on whether that source supports music fetching.
/// YouTube returns Visible; other sources return Collapsed.
/// </summary>
public class CanFetchMusicToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a SourceFlag to Visibility based on music support.
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
                    return Visibility.Collapsed;
                case SourceFlag.Youtube:
                    return Visibility.Visible;
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
