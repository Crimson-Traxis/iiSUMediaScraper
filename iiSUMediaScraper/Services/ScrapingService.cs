using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Models.Extensions;
using iiSUMediaScraper.Scrapers;
using iiSUMediaScraper.Scrapers.Igdb;
using iiSUMediaScraper.Scrapers.Ign;
using iiSUMediaScraper.Scrapers.SteamGridDb;
using iiSUMediaScraper.Scrapers.Youtube;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Coordinates multiple media scrapers to fetch game artwork and metadata from various sources.
/// Aggregates and prioritizes media from IGN, SteamGridDB, and IGDB.
/// </summary>
public class ScrapingService : IScrapingService
{
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Initializes a new instance of the ScrapingService.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="configurationService">Service for accessing application configuration.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public ScrapingService(IHttpClientFactory httpClientFactory, IConfigurationService configurationService, IDownloader downloader, ILogger<ScrapingService> logger)
    {
        HttpClientFactory = httpClientFactory;
        ConfigurationService = configurationService;
        Downloader = downloader;
        Logger = logger;
    }

    /// <summary>
    /// Fetches media (icons, logos, titles, heroes, slides) for a game from multiple sources.
    /// Runs scrapers in parallel, then re-scrapes if needed, and aggregates results by priority.
    /// </summary>
    /// <param name="platform">The gaming platform (e.g., "PS2", "Switch").</param>
    /// <param name="game">The name of the game.</param>
    /// <returns>Aggregated media context with deduplicated and prioritized media.</returns>
    public async Task<MediaContext> GetMedia(string platform, string game)
    {
        try
        {
            Configuration? configuration = ConfigurationService.Configuration;

            if (configuration != null)
            {
                return await Task.Run(async () =>
                {
                    // Initialize all scrapers for different media sources
                    var ignScraper = new IgnScraper(HttpClientFactory, Downloader, configuration, Logger);
                    var steamGridDbScraper = new SteamGridDbScraper(HttpClientFactory, Downloader, configuration, Logger);
                    var igdbScraper = new IgdbScraper(HttpClientFactory, Downloader, configuration, Logger);
                    var youtubeScraper = new YoutubeScraper(HttpClientFactory, Downloader, configuration, Logger);

                    // Phase 1: Run all scrapers in parallel for initial media fetch
                    var ignScraperTask = ignScraper.ScrapeMedia(platform, game);
                    var steamGridDbScraperTask = steamGridDbScraper.ScrapeMedia(platform, game);
                    var igdbScraperTask = igdbScraper.ScrapeMedia(platform, game);
                    var youtubeScraperTask = youtubeScraper.ScrapeMedia(platform, game);

                    await Task.WhenAll([ignScraperTask, steamGridDbScraperTask, igdbScraperTask, youtubeScraperTask]).ConfigureAwait(false);

                    // Collect results from first pass, filtering out nulls
                    var mediaContexts = new List<MediaContext?>
                                                    {
                                                        await ignScraperTask,
                                                        await steamGridDbScraperTask,
                                                        await igdbScraperTask,
                                                        await youtubeScraperTask,
                                                    }.OfType<MediaContext>();

                    // Phase 2: Re-scrape with knowledge of what media was found
                    // This allows scrapers to fill in missing media types
                    ignScraperTask = ignScraper.ScrapeMedia(platform, game, mediaContexts.Flatten());
                    steamGridDbScraperTask = steamGridDbScraper.ScrapeMedia(platform, game, mediaContexts.Flatten());
                    igdbScraperTask = igdbScraper.ScrapeMedia(platform, game, mediaContexts.Flatten());

                    // YouTube only needs one pass since it doesn't have fallback logic for images

                    await Task.WhenAll([ignScraperTask, steamGridDbScraperTask, igdbScraperTask]).ConfigureAwait(false);

                    var mediaContextsReScrape = new List<MediaContext?>
                                                    {
                                                        await ignScraperTask,
                                                        await steamGridDbScraperTask,
                                                        await igdbScraperTask
                                                    }.OfType<MediaContext>();

                    // Combine results from both passes
                    mediaContexts = [.. mediaContexts, .. mediaContextsReScrape];

                    // Aggregate and prioritize media from all sources
                    // Deduplicate by URL and sort by configured priority (lower = higher priority)
                    IOrderedEnumerable<Image> icons = mediaContexts.Select(m => m.Icons)
                        .SelectMany(i => i)
                        .DistinctBy(i => i.Url)
                        .OrderBy(i => i.GetIconPriority(configuration))
                        .ThenBy(i => i.GetSquareIconPriority(configuration));

                    IOrderedEnumerable<Image> titles = mediaContexts.Select(m => m.Titles)
                        .SelectMany(t => t)
                        .DistinctBy(t => t.Url)
                        .OrderBy(t => t.GetTitlePriority(configuration))
                        .ThenBy(t => t.GetSquareIconPriority(configuration));

                    IOrderedEnumerable<Image> logos = mediaContexts.Select(m => m.Logos)
                        .SelectMany(l => l)
                        .DistinctBy(l => l.Url)
                        .OrderBy(l => l.GetLogoPriority(configuration));

                    IOrderedEnumerable<Image> heros = mediaContexts.Select(m => m.Heros)
                        .SelectMany(h => h)
                        .DistinctBy(h => h.Url)
                        .OrderBy(h => h.GetHeroPriority(configuration));

                    IOrderedEnumerable<Image> slides = mediaContexts.Select(m => m.Slides)
                        .SelectMany(s => s)
                        .DistinctBy(s => s.Url)
                        .OrderBy(s => s.GetSlidePriority(configuration));

                    // Music is sorted by search term priority first, then by like count
                    IEnumerable<Music> music = mediaContexts.Select(m => m.Music)
                        .SelectMany(s => s)
                        .DistinctBy(s => s.Url)
                        .OrderBy(s => s.GetMusicPriority(configuration).TermPriority)
                        .ThenByDescending(s => s.GetMusicPriority(configuration).LikeCount);

                    // Videos are sorted by popularity (like count)
                    IEnumerable<Video> videos = mediaContexts.Select(m => m.Videos)
                        .SelectMany(s => s)
                        .DistinctBy(s => s.Url)
                        .OrderByDescending(s => s.LikeCount);

                    return new MediaContext()
                    {
                        Icons = [.. icons],
                        Titles = [.. titles],
                        Logos = [.. logos],
                        Heros = [.. heros],
                        Slides = [.. slides],
                        Music = [.. music],
                        Videos = [.. videos],
                    };
                });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get media for platform: {Platform}, game: {Game}", platform, game);
        }

        return new MediaContext();
    }

    /// <summary>
    /// Downloads all media from a MediaContext that hasn't been downloaded yet.
    /// Downloads icons, logos, titles, heroes, and slides in parallel.
    /// </summary>
    /// <param name="mediaContext">The media context containing URLs to download.</param>
    public Task DownloadMissingMedia(MediaContext mediaContext)
    {
        return Task.Run(async () =>
        {
            try
            {
                // Create per-request cache that gets disposed after downloading
                var mediaCache = new ConcurrentDictionary<string, Media>();

                var downloadTasks = new List<Task>();

                foreach (Image media in mediaContext.Icons)
                {
                    downloadTasks.Add(Downloader.DownloadImage(media));
                }
                foreach (Image media in mediaContext.Logos)
                {
                    downloadTasks.Add(Downloader.DownloadImage(media));
                }
                foreach (Image media in mediaContext.Titles)
                {
                    downloadTasks.Add(Downloader.DownloadImage(media));
                }
                foreach (Image media in mediaContext.Heros)
                {
                    downloadTasks.Add(Downloader.DownloadImage(media));
                }
                foreach (Image media in mediaContext.Slides)
                {
                    downloadTasks.Add(Downloader.DownloadImage(media));
                }

                await Task.WhenAll(downloadTasks);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to download missing media");
            }
        });
    }

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }

    /// <summary>
    /// Gets the HTTP client factory for creating HTTP clients.
    /// </summary>
    protected IHttpClientFactory HttpClientFactory { get; private set; }

    /// <summary>
    /// Gets the downloader.
    /// </summary>
    protected IDownloader Downloader { get; private set; }

    /// <summary>
    /// Gets the configuration service for accessing scraper settings and API keys.
    /// </summary>
    protected IConfigurationService ConfigurationService { get; private set; }
}
