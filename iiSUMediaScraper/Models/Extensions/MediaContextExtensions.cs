namespace iiSUMediaScraper.Models.Extensions;

/// <summary>
/// Extension methods for MediaContext operations.
/// </summary>
public static class MediaContextExtensions
{
    /// <summary>
    /// Flattens multiple media contexts into a single context by combining all collections.
    /// </summary>
    /// <param name="mediaContexts">The collection of media contexts to flatten.</param>
    /// <returns>A new MediaContext with all media items combined.</returns>
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
