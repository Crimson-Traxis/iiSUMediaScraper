using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.Models.Extensions;

/// <summary>
/// Extension methods for media priority calculations.
/// </summary>
public static class MediaExtensions
{
    /// <summary>
    /// Gets the icon priority for a media item based on configuration.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The priority value (lower is higher priority).</returns>
    public static int GetIconPriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Icon", configuration);
    }

    /// <summary>
    /// Gets the logo priority for a media item based on configuration.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The priority value (lower is higher priority).</returns>
    public static int GetLogoPriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Logo", configuration);
    }

    /// <summary>
    /// Gets the title priority for a media item based on configuration.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The priority value (lower is higher priority).</returns>
    public static int GetTitlePriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Title", configuration);
    }

    /// <summary>
    /// Gets the hero priority for a media item based on configuration.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The priority value (lower is higher priority).</returns>
    public static int GetHeroPriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Hero", configuration);
    }

    /// <summary>
    /// Gets the slide priority for a media item based on configuration.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The priority value (lower is higher priority).</returns>
    public static int GetSlidePriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Slide", configuration);
    }

    /// <summary>
    /// Gets the music priority based on search term matching and like count.
    /// </summary>
    /// <param name="music">The music item.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>A tuple of term priority and like count for sorting.</returns>
    public static (int TermPriority, long LikeCount) GetMusicPriority(this Music music, Configuration configuration)
    {
        var priorities = new long[2] { 0,  music.LikeCount };

        if (!string.IsNullOrWhiteSpace(configuration.MusicSearchTermPriority))
        {
            var matchIndex = 0;

            foreach (var term in configuration.MusicSearchTermPriority.Split(",", StringSplitOptions.TrimEntries))
            {
                if (music.Title?.Contains(term, StringComparison.CurrentCultureIgnoreCase) ?? false)
                {
                    break;
                }

                matchIndex++;
            }

            priorities[0] = matchIndex;
        }

        return ((int)priorities[0], priorities[1]);
    }

    /// <summary>
    /// Gets the priority for a media item based on type and scraper configuration.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="type">The media type (Icon, Logo, Title, Hero, Slide).</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The priority value (lower is higher priority).</returns>
    public static int GetMediaPriority(this Media media, string type, Configuration configuration)
    {
        if (configuration.ScraperConfigurations.FirstOrDefault(c => c.Source == media.Source) is ScraperConfiguration scraperConfiguration)
        {
            switch (type)
            {
                case "Icon":
                    return scraperConfiguration.IconPriority;
                case "Logo":
                    return scraperConfiguration.LogoPriority;
                case "Title":
                    return scraperConfiguration.TitlePriority;
                case "Hero":
                    return scraperConfiguration.HeroPriority;
                case "Slide":
                    return scraperConfiguration.SlidePriority;
            }
        }

        return int.MaxValue;
    }

    /// <summary>
    /// Gets the square icon priority, favoring square images when configured.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>0 for square images, 1 for non-square, or max value if not applicable.</returns>
    public static int GetSquareIconPriority(this Media media, Configuration configuration)
    {
        if (configuration.ScraperConfigurations.FirstOrDefault(c => c.Source == media.Source) is ScraperConfiguration scraperConfiguration)
        {
            if (scraperConfiguration.IsUseSquareIconPriority)
            {
                if (media is Image image)
                {
                    return image.Width == image.Height ? 0 : 1;
                }
            }
        }

        return int.MaxValue;
    }
}
