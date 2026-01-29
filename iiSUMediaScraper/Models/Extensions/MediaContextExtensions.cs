namespace iiSUMediaScraper.Models.Extensions;

public static class MediaContextExtensions
{
    public static MediaContext Flatten(this IEnumerable<MediaContext> mediaContexts)
    {
        return new MediaContext
        {
            Icons = [.. mediaContexts.Select(m => m.Icons).SelectMany(m => m)],

            Logos = [.. mediaContexts.Select(m => m.Logos).SelectMany(m => m)],

            Titles = [.. mediaContexts.Select(m => m.Titles).SelectMany(m => m)],

            Heros = [.. mediaContexts.Select(m => m.Heros).SelectMany(m => m)],

            Slides = [.. mediaContexts.Select(m => m.Slides).SelectMany(m => m)]
        };
    }
}
