using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Models.Extensions;
using iiSUMediaScraper.Scrapers;
using iiSUMediaScraper.Scrapers.Igdb;
using iiSUMediaScraper.Scrapers.Ign;
using iiSUMediaScraper.Scrapers.SteamGridDb;
using Microsoft.Extensions.Logging;

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
    public ScrapingService(IHttpClientFactory httpClientFactory, IConfigurationService configurationService, ILogger<ScrapingService> logger)
    {
        HttpClientFactory = httpClientFactory;
        ConfigurationService = configurationService;
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
                    var ignScraper = new IgnScraper(HttpClientFactory, configuration, Logger);
                    var steamGridDbScraper = new SteamGridDbScraper(HttpClientFactory, configuration, Logger);
                    var igdbScraper = new IgdbScraper(HttpClientFactory, configuration, Logger);

                    Task<MediaContext?> ignScraperTask = ignScraper.ScrapeMedia(platform, game);
                    Task<MediaContext?> steamGridDbScraperTask = steamGridDbScraper.ScrapeMedia(platform, game);
                    Task<MediaContext?> igdbScraperTask = igdbScraper.ScrapeMedia(platform, game);

                    await Task.WhenAll([ignScraperTask, steamGridDbScraperTask, igdbScraperTask]).ConfigureAwait(false);

                    IEnumerable<MediaContext> mediaContexts = new List<MediaContext?>
                                                    {
                                                        await ignScraperTask,
                                                        await steamGridDbScraperTask,
                                                        await igdbScraperTask
                                                    }.OfType<MediaContext>();

                    ignScraperTask = ignScraper.ScrapeMedia(platform, game, mediaContexts.Flatten());
                    steamGridDbScraperTask = steamGridDbScraper.ScrapeMedia(platform, game, mediaContexts.Flatten());
                    igdbScraperTask = igdbScraper.ScrapeMedia(platform, game, mediaContexts.Flatten());

                    await Task.WhenAll([ignScraperTask, steamGridDbScraperTask, igdbScraperTask]).ConfigureAwait(false);

                    IEnumerable<MediaContext> mediaContextsReScrape = new List<MediaContext?>
                                                    {
                                                        await ignScraperTask,
                                                        await steamGridDbScraperTask,
                                                        await igdbScraperTask
                                                    }.OfType<MediaContext>();

                    mediaContexts = [.. mediaContexts, .. mediaContextsReScrape];

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

                    IOrderedEnumerable<Media> heros = mediaContexts.Select(m => m.Heros)
                        .SelectMany(h => h)
                        .DistinctBy(h => h.Url)
                        .OrderBy(h => h.GetHeroPriority(configuration));

                    IOrderedEnumerable<Media> slides = mediaContexts.Select(m => m.Slides)
                        .SelectMany(s => s)
                        .DistinctBy(s => s.Url)
                        .OrderBy(s => s.GetSlidePriority(configuration));

                    return new MediaContext()
                    {
                        Icons = [.. icons],
                        Titles = [.. titles],
                        Logos = [.. logos],
                        Heros = [.. heros],
                        Slides = [.. slides]
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
                var downloader = new Downloader(HttpClientFactory, Logger);

                List<Task> downloadTasks = new List<Task>();

                foreach (Image media in mediaContext.Icons)
                {
                    downloadTasks.Add(downloader.DownloadMedia(media));
                }

                foreach (Image media in mediaContext.Logos)
                {
                    downloadTasks.Add(downloader.DownloadMedia(media));
                }

                foreach (Image media in mediaContext.Titles)
                {
                    downloadTasks.Add(downloader.DownloadMedia(media));
                }

                foreach (Media media in mediaContext.Heros)
                {
                    downloadTasks.Add(downloader.DownloadMedia(media));
                }

                foreach (Media media in mediaContext.Slides)
                {
                    downloadTasks.Add(downloader.DownloadMedia(media));
                }

                await Task.WhenAll(downloadTasks);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to download missing media");
            }
        });
    }

    protected ILogger Logger { get; private set; }

    /// <summary>
    /// Gets the HTTP client factory for creating HTTP clients.
    /// </summary>
    protected IHttpClientFactory HttpClientFactory { get; private set; }

    /// <summary>
    /// Gets the configuration service for accessing scraper settings and API keys.
    /// </summary>
    protected IConfigurationService ConfigurationService { get; private set; }
}
