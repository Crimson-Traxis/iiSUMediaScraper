using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace iiSUMediaScraper.Scrapers;

/// <summary>
/// Base class for all media scrapers. Provides common functionality for title matching,
/// name sanitization, and media downloading.
/// </summary>
public abstract class Scraper
{
    private readonly IDownloader _downloader;

    /// <summary>
    /// Maximum number of retry attempts for failed requests.
    /// </summary>
    private const int MaxRetries = 5;

    /// <summary>
    /// Initializes a new instance of the Scraper.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="mediaCache">Shared media cache for this scraping session.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public Scraper(IHttpClientFactory httpClientFactory, IDownloader downloader, Configuration configuration, ILogger logger)
    {
        HttpClientFactory = httpClientFactory;

        GlobalConfiguration = configuration;

        _downloader = downloader;

        Logger = logger;
    }

    /// <summary>
    /// Converts Roman numerals (I-XX) to Arabic numbers in a string.
    /// Used for title matching when games use different numbering systems.
    /// </summary>
    /// <param name="text">Text containing potential Roman numerals.</param>
    /// <returns>Text with Roman numerals converted to numbers.</returns>
    private string ConvertRomanToNumbers(string text)
    {
        var romanMap = new Dictionary<string, string>
        {
            { "XX", "20" },
            { "XIX", "19" },
            { "XVIII", "18" },
            { "XVII", "17" },
            { "XVI", "16" },
            { "XV", "15" },
            { "XIV", "14" },
            { "XIII", "13" },
            { "XII", "12" },
            { "XI", "11" },
            { "IX", "9" },
            { "VIII", "8" },
            { "VII", "7" },
            { "VI", "6" },
            { "IV", "4" },
            { "III", "3" },
            { "II", "2" },
            { "X", "10" },
            { "V", "5" },
            { "I", "1" }
        };

        string result = text;

        // Process in order (longer to shorter) to match correctly
        foreach (KeyValuePair<string, string> kvp in romanMap)
        {
            // Match Roman numerals that are standalone (word boundaries)
            // Case-insensitive match
            result = Regex.Replace(result, $@"\b{kvp.Key}\b", kvp.Value, RegexOptions.IgnoreCase);
        }

        return result;
    }

    /// <summary>
    /// Converts Arabic numbers (1-20) to Roman numerals in a string.
    /// Used for title matching when games use different numbering systems.
    /// </summary>
    /// <param name="text">Text containing numbers.</param>
    /// <returns>Text with numbers converted to Roman numerals.</returns>
    private string ConvertNumbersToRoman(string text)
    {
        var numberMap = new Dictionary<string, string>
        {
            { "20", "XX" },
            { "19", "XIX" },
            { "18", "XVIII" },
            { "17", "XVII" },
            { "16", "XVI" },
            { "15", "XV" },
            { "14", "XIV" },
            { "13", "XIII" },
            { "12", "XII" },
            { "11", "XI" },
            { "10", "X" },
            { "9", "IX" },
            { "8", "VIII" },
            { "7", "VII" },
            { "6", "VI" },
            { "5", "V" },
            { "4", "IV" },
            { "3", "III" },
            { "2", "II" },
            { "1", "I" }
        };

        string result = text;

        // Process in order (20 to 1) to match longer numbers first
        foreach (KeyValuePair<string, string> kvp in numberMap)
        {
            // Match numbers that are standalone (word boundaries)
            result = Regex.Replace(result, $@"\b{kvp.Key}\b", kvp.Value);
        }

        return result;
    }

    /// <summary>
    /// Removes diacritical marks (accents) from characters.
    /// Converts "Pokémon" to "Pokemon" for better title matching.
    /// </summary>
    /// <param name="text">Text to normalize.</param>
    /// <returns>Text with accents removed.</returns>
    private string NormalizeAccentedCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Normalize to FormD (decomposed form) which separates base characters from diacritics
        string normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            // Get the Unicode category of the character
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

            // Keep all characters except non-spacing marks (diacritics)
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Normalize back to FormC (composed form)
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Downloads an image and removes it from the list if download fails.
    /// Sets the source flag on successful download.
    /// </summary>
    /// <param name="medias">The list containing the image.</param>
    /// <param name="media">The image to download.</param>
    private async Task DownloadAndCheckMedia(IList<Image> medias, Image media)
    {
        media.Source = Source;

        if (!await DownloadImage(media))
        {
            medias.Remove(media);
        }
    }

    /// <summary>
    /// Determines if two game titles match, applying progressively aggressive sanitization.
    /// Tries matching with: original names, without regions, without special chars, and with Roman/Arabic number conversion.
    /// </summary>
    /// <param name="sourceTitle">The original game title (from file name).</param>
    /// <param name="foundTitle">The title found from the scraper.</param>
    /// <returns>True if the titles match at any sanitization level.</returns>
    protected bool TitleMatches(string sourceTitle, string? foundTitle)
    {
        if (string.IsNullOrWhiteSpace(foundTitle))
        {
            return false;
        }

        // Normalize both titles to lowercase and remove accents for comparison
        sourceTitle = NormalizeAccentedCharacters(sourceTitle.ToLower());

        foundTitle = NormalizeAccentedCharacters(foundTitle.ToLower());

        // Try matching at progressively more aggressive sanitization levels
        // Starting with exact match, then removing regions, special chars, etc.
        List<SanitizationLevel> levels =
        [
            SanitizationLevel.None,
            SanitizationLevel.Region,
            SanitizationLevel.RegionAndSpecialCharacters,
            SanitizationLevel.RegionAndSpecialCharactersAndNoSpaces,
            SanitizationLevel.RegionAndSpecialCharactersAndRomanNumerals,
            SanitizationLevel.RegionAndSpecialCharactersAndNoSpacesAndRomanNumerals
        ];

        foreach (SanitizationLevel level in levels)
        {
            int matchCount = 0;

            string sanitizedSourceTitle = SanitizeName(sourceTitle, level);

            string sanitizedFoundTitle = SanitizeName(foundTitle, level);

            // Split the found title into words and check if all words exist in the source title
            // This allows for partial matches where word order may differ
            string[] parts = sanitizedFoundTitle.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (sanitizedSourceTitle.Contains(part))
                {
                    matchCount++;
                }
            }

            // Match found if all words from the found title exist in the source title
            if (matchCount == parts.Length)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes file extension and parenthetical region/version info from a game name.
    /// Example: "Super Mario Bros (USA).nes" becomes "Super Mario Bros"
    /// </summary>
    /// <param name="name">The game name to clean.</param>
    /// <returns>Cleaned game name.</returns>
    protected string CleanName(string name)
    {
        // Remove only known file extensions
        var extensions = GlobalConfiguration.ExtensionConfigurations.SelectMany(e => e.Extension);

        foreach (string ext in extensions)
        {
            string extension = ext.StartsWith('.') ? ext : "." + ext;
            if (name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                name = name[..^extension.Length];
                break;
            }
        }

        // Remove only trailing parenthetical groups (region info, version, etc.)
        string pattern = @"(\s*\([^)]*\))+$";
        name = Regex.Replace(name, pattern, string.Empty).Trim();

        return name;
    }

    /// <summary>
    /// Scrapes media for a specific game from the scraper's data source.
    /// Must be implemented by derived scrapers.
    /// </summary>
    /// <param name="platformId">Platform identifier specific to the scraper.</param>
    /// <param name="name">Cleaned game name.</param>
    /// <param name="previous">Previous media context to determine what additional media to fetch.</param>
    /// <returns>Media context containing scraped media, or null if not found.</returns>
    protected abstract Task<MediaContext?> OnScrapeMedia(string platformId, string name, MediaContext? previous = null);

    /// <summary>
    /// Downloads image using the internal downloader.
    /// </summary>
    /// <param name="image">The image to download.</param>
    /// <returns>True if download succeeded.</returns>
    protected Task<bool> DownloadImage(Image image)
    {
        return _downloader.DownloadImage(image);
    }

    /// <summary>
    /// Sends an HTTP request with retry logic.
    /// Retries up to 5 times on timeout, transient errors (429, 500-504).
    /// Uses exponential backoff between retries.
    /// Timeout is configured on the HttpClient in App.xaml.cs.
    /// </summary>
    /// <param name="client">The HttpClient to use for the request.</param>
    /// <param name="requestMessageFactory">Factory function to create request messages (needed for retries since requests cannot be reused).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    /// <exception cref="HttpRequestException">Thrown when all retry attempts fail.</exception>
    protected async Task<HttpResponseMessage> SendWithRetryAsync(
        HttpClient client,
        Func<HttpRequestMessage> requestMessageFactory,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                using var requestMessage = requestMessageFactory();
                var response = await client.SendAsync(requestMessage, cancellationToken);

                // Check for transient HTTP errors that should be retried
                if (IsTransientError(response.StatusCode))
                {
                    lastException = new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.StatusCode}");
                    Logger.LogDebug("Transient error {StatusCode} on attempt {Attempt}/{MaxRetries}, retrying...",
                        (int)response.StatusCode, attempt + 1, MaxRetries);

                    await DelayBeforeRetry(attempt, cancellationToken);
                    continue;
                }

                return response;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // This was a timeout from HttpClient, not a user cancellation
                lastException = new TimeoutException("Request timed out", ex);
                Logger.LogDebug("Request timeout on attempt {Attempt}/{MaxRetries}, retrying...", attempt + 1, MaxRetries);

                await DelayBeforeRetry(attempt, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                Logger.LogDebug(ex, "HTTP error on attempt {Attempt}/{MaxRetries}, retrying...", attempt + 1, MaxRetries);

                await DelayBeforeRetry(attempt, cancellationToken);
            }
        }

        Logger.LogWarning(lastException, "All {MaxRetries} retry attempts failed", MaxRetries);
        throw new HttpRequestException($"Request failed after {MaxRetries} attempts", lastException);
    }

    /// <summary>
    /// Determines if an HTTP status code represents a transient error that should be retried.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the error is transient and should be retried.</returns>
    private static bool IsTransientError(System.Net.HttpStatusCode statusCode)
    {
        return statusCode == System.Net.HttpStatusCode.TooManyRequests ||          // 429
               statusCode == System.Net.HttpStatusCode.InternalServerError ||       // 500
               statusCode == System.Net.HttpStatusCode.BadGateway ||                // 502
               statusCode == System.Net.HttpStatusCode.ServiceUnavailable ||        // 503
               statusCode == System.Net.HttpStatusCode.GatewayTimeout;              // 504
    }

    /// <summary>
    /// Delays before the next retry attempt using exponential backoff.
    /// </summary>
    /// <param name="attempt">The current attempt number (0-based).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private static async Task DelayBeforeRetry(int attempt, CancellationToken cancellationToken)
    {
        // Exponential backoff: 1s, 2s, 4s, 8s, 16s
        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
        await Task.Delay(delay, cancellationToken);
    }

    /// <summary>
    /// Sanitizes a game name by removing unwanted characters based on the specified level.
    /// </summary>
    /// <param name="name">The name to sanitize.</param>
    /// <param name="level">The level of sanitization to apply.</param>
    /// <returns>Sanitized name.</returns>
    protected string SanitizeName(string name, SanitizationLevel level)
    {
        // Step 1: Remove trailing parenthetical content like (USA), (Europe), (Rev 1)
        // and normalize multiple spaces to single space
        if (level is SanitizationLevel.Region or
            SanitizationLevel.RegionAndSpecialCharacters or
            SanitizationLevel.RegionAndSpecialCharactersAndNoSpaces or
            SanitizationLevel.RegionAndSpecialCharactersAndRomanNumerals or
            SanitizationLevel.RegionAndSpecialCharactersAndNoSpacesAndRomanNumerals)
        {
            name = Regex.Replace(Regex.Replace(name, @"\s*\([^)]*\)$", ""), @"\s+", " ");
        }

        // Step 2: Remove all non-alphanumeric characters except spaces
        if (level is SanitizationLevel.RegionAndSpecialCharacters or
            SanitizationLevel.RegionAndSpecialCharactersAndNoSpaces or
            SanitizationLevel.RegionAndSpecialCharactersAndRomanNumerals or
            SanitizationLevel.RegionAndSpecialCharactersAndNoSpacesAndRomanNumerals)
        {
            name = Regex.Replace(name, @"[^a-zA-Z0-9\s]", "").Trim();
        }

        // Step 3: Normalize Roman numerals by converting to Arabic and back
        // This handles cases like "Final Fantasy III" vs "Final Fantasy 3"
        if (level is SanitizationLevel.RegionAndSpecialCharactersAndRomanNumerals or
            SanitizationLevel.RegionAndSpecialCharactersAndNoSpacesAndRomanNumerals)
        {
            name = ConvertRomanToNumbers(name);
            name = ConvertNumbersToRoman(name);
        }

        // Step 4: Remove all spaces for the most aggressive matching
        if (level is SanitizationLevel.RegionAndSpecialCharactersAndNoSpaces or
            SanitizationLevel.RegionAndSpecialCharactersAndNoSpacesAndRomanNumerals)
        {
            name = Regex.Replace(name, @"\s+", "");
        }

        return name;
    }

    /// <summary>
    /// Main entry point for scraping media. Handles platform translation, downloading, and fallback logic.
    /// </summary>
    /// <param name="platform">Platform name (e.g., "PS2", "Switch").</param>
    /// <param name="name">Game name from the file.</param>
    /// <param name="previous">Previous scrape results to determine what additional media to fetch.</param>
    /// <returns>Media context with all scraped and downloaded media.</returns>
    public async Task<MediaContext?> ScrapeMedia(string platform, string name, MediaContext? previous = null)
    {
        try
        {
            if (Configuration.IsFetch)
            {
                // Translate platform name to scraper-specific identifier (e.g., "PS2" -> "playstation-2")
                var platformIdentifier = Configuration.PlatformTranslationConfigurations.FirstOrDefault(c => c.Platform == platform)?.Identifier ?? platform;

                Logger.LogDebug("Scraping media for {Game} on platform {Platform} using {Source}", name, platform, Source);

                // Call the scraper-specific implementation to fetch media URLs
                var context = await OnScrapeMedia(platformIdentifier, CleanName(name), previous);

                if (context != null)
                {
                    // Fallback: use title images as icons if no icons were found
                    if (Configuration.IsAllowTitleAsIconWhenNoIconFound)
                    {
                        if (context.Icons.Count == 0)
                        {
                            foreach (var title in context.Titles)
                            {
                                context.Icons.Add(new Image()
                                {
                                    Url = title.Url,
                                    Source = title.Source,
                                });
                            }
                        }
                    }

                    // Apply fetch limits before downloading
                    if (Configuration.IconFetchLimit is int iconLimit)
                        context.Icons = [.. context.Icons.Take(iconLimit)];

                    if (Configuration.LogoFetchLimit is int logoLimit)
                        context.Logos = [.. context.Logos.Take(logoLimit)];

                    if (Configuration.TitleFetchLimit is int titleLimit)
                        context.Titles = [.. context.Titles.Take(titleLimit)];

                    if (Configuration.HeroFetchLimit is int heroLimit)
                        context.Heros = [.. context.Heros.Take(heroLimit)];

                    if (Configuration.SlideFetchLimit is int slideLimit)
                        context.Slides = [.. context.Slides.Take(slideLimit)];

                    // Download all media in parallel
                    // Create copies of lists since downloads may remove failed items
                    List<Task> downloadTasks = [];

                    List<Image> icons = [.. context.Icons];

                    foreach (var media in context.Icons)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(icons, media));
                    }

                    List<Image> logos = [.. context.Logos];

                    foreach (var media in context.Logos)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(logos, media));
                    }

                    List<Image> titles = [.. context.Titles];

                    foreach (var media in context.Titles)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(titles, media));
                    }

                    List<Image> heros = [.. context.Heros];

                    foreach (var media in context.Heros)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(heros, media));
                    }

                    List<Image> slides = [.. context.Slides];

                    foreach (var media in context.Slides)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(slides, media));
                    }

                    await Task.WhenAll(downloadTasks);

                    // Update context with successfully downloaded media only
                    context.Icons = icons;
                    context.Logos = logos;
                    context.Titles = titles;
                    context.Heros = heros;
                    context.Slides = slides;
                    context.Music = [.. context.Music];
                    context.Videos = [.. context.Videos];

                    Logger.LogDebug("Found {IconCount} icons, {LogoCount} logos, {TitleCount} titles, {HeroCount} heros, {SlideCount} slides from {Source}",
                        context.Icons.Count, context.Logos.Count, context.Titles.Count, context.Heros.Count, context.Slides.Count, Source);
                }

                HasScraped = true;

                return context;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to scrape media for {Game} on platform {Platform} using {Source}", name, platform, Source);
        }

        return default;
    }

    /// <summary>
    /// Gets the global application configuration.
    /// </summary>
    protected Configuration GlobalConfiguration { get; private set; }

    /// <summary>
    /// Gets the scraper-specific configuration for this scraper instance.
    /// </summary>
    protected virtual ScraperConfiguration Configuration
    {
        get
        {
            if (GlobalConfiguration.ScraperConfigurations.FirstOrDefault(c => c.Source == Source) is ScraperConfiguration configuration)
            {
                return configuration;
            }

            return new ScraperConfiguration();
        }
    }

    /// <summary>
    /// Gets the HTTP client factory for making web requests.
    /// </summary>
    protected IHttpClientFactory HttpClientFactory { get; private set; }

    /// <summary>
    /// Defines levels of name sanitization for matching game titles.
    /// </summary>
    public enum SanitizationLevel
    {
        /// <summary>
        /// No sanitization applied.
        /// </summary>
        None,

        /// <summary>
        /// Remove region info like (USA), (Europe).
        /// </summary>
        Region,

        /// <summary>
        /// Remove region info and special characters.
        /// </summary>
        RegionAndSpecialCharacters,

        /// <summary>
        /// Remove region info, special characters, and no spaces.
        /// </summary>
        RegionAndSpecialCharactersAndNoSpaces,

        /// <summary>
        /// Remove region info, special chars, and convert between Roman/Arabic numerals.
        /// </summary>
        RegionAndSpecialCharactersAndRomanNumerals,

        /// <summary>
        /// Remove region info, special chars, no spaces, and convert between Roman/Arabic numerals.
        /// </summary>
        RegionAndSpecialCharactersAndNoSpacesAndRomanNumerals,
    }

    /// <summary>
    /// Gets the source flag identifying this scraper.
    /// </summary>
    protected abstract SourceFlag Source { get; }

    /// <summary>
    /// Gets or sets whether this scraper has successfully scraped data.
    /// Used to optimize subsequent scrapes for the same game.
    /// </summary>
    protected bool HasScraped { get; set; }

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }

    /// <summary>
    /// Gets the folder path where external tools (yt-dlp.exe, ffmpeg.exe, ffprobe.exe, ffplay.exe) are located.
    /// </summary>
    protected string ToolsFolder => _downloader.ToolsFolder;
}
