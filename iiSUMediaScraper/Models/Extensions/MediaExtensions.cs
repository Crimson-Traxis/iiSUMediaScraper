using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.Models.Extensions;

public static class MediaExtensions
{
    public static int GetIconPriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Icon", configuration);
    }

    public static int GetLogoPriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Logo", configuration);
    }

    public static int GetTitlePriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Title", configuration);
    }

    public static int GetHeroPriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Hero", configuration);
    }

    public static int GetSlidePriority(this Media media, Configuration configuration)
    {
        return GetMediaPriority(media, "Slide", configuration);
    }

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
