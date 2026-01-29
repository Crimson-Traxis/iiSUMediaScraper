using Microsoft.Windows.ApplicationModel.Resources;

namespace iiSUMediaScraper.Helpers;

/// <summary>
/// Extension methods for loading localized string resources.
/// </summary>
public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();

    /// <summary>
    /// Retrieves a localized string for the specified resource key.
    /// </summary>
    /// <param name="resourceKey">The key identifying the localized resource.</param>
    /// <returns>The localized string.</returns>
    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);
}
