using iiSUMediaScraper.Models;

namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service for scraping media from various sources for games.
/// </summary>
public interface IScrapingService
{
    /// <summary>
    /// Gets media for a game from all configured scraper sources.
    /// </summary>
    /// <param name="platform">The gaming platform.</param>
    /// <param name="game">The game name to search for.</param>
    /// <returns>A MediaContext containing all found media.</returns>
    Task<MediaContext> GetMedia(string platform, string game);

    /// <summary>
    /// Downloads any media that has not yet been downloaded locally.
    /// </summary>
    /// <param name="mediaContext">The media context containing media to download.</param>
    Task DownloadMissingMedia(MediaContext mediaContext);
}
