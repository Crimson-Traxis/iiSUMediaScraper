namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service for mapping view model keys to their corresponding page types.
/// </summary>
public interface IPageService
{
    /// <summary>
    /// Gets the page type associated with the specified key.
    /// </summary>
    /// <param name="key">The view model's full type name.</param>
    /// <returns>The corresponding page type.</returns>
    Type GetPageType(string key);
}
