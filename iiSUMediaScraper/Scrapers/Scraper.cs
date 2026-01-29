using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;
using Microsoft.Extensions.Logging;
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
    private readonly Downloader _downloader;

    /// <summary>
    /// Initializes a new instance of the Scraper.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public Scraper(IHttpClientFactory httpClientFactory, Configuration configuration, ILogger logger)
    {
        HttpClientFactory = httpClientFactory;

        GlobalConfiguration = configuration;

        Logger = logger;

        _downloader = new Downloader(httpClientFactory, logger);
    }

    /// <summary>
    /// Converts Roman numerals (I-XX) to Arabic numbers in a string.
    /// Used for title matching when games use different numbering systems.
    /// </summary>
    /// <param name="text">Text containing potential Roman numerals.</param>
    /// <returns>Text with Roman numerals converted to numbers.</returns>
    private string ConvertRomanToNumbers(string text)
    {
        Dictionary<string, string> romanMap = new Dictionary<string, string>
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
        Dictionary<string, string> numberMap = new Dictionary<string, string>
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
        StringBuilder stringBuilder = new StringBuilder();

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
    /// Downloads media and removes it from the list if download fails.
    /// Sets the source flag on successful download.
    /// </summary>
    /// <param name="medias">The list containing the media.</param>
    /// <param name="media">The media to download.</param>
    private async Task DownloadAndCheckMedia(IList<Media> medias, Media media)
    {
        if (!await DownloadMedia(media))
        {
            medias.Remove(media);
        }

        media.Source = Source;
    }

    /// <summary>
    /// Downloads an image and removes it from the list if download fails.
    /// Sets the source flag on successful download.
    /// </summary>
    /// <param name="medias">The list containing the image.</param>
    /// <param name="media">The image to download.</param>
    private async Task DownloadAndCheckMedia(IList<Image> medias, Image media)
    {
        if (!await DownloadMedia(media))
        {
            medias.Remove(media);
        }

        media.Source = Source;
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

        sourceTitle = NormalizeAccentedCharacters(sourceTitle.ToLower());

        foundTitle = NormalizeAccentedCharacters(foundTitle.ToLower());

        List<SanitizationLevel> levels =
        [
            SanitizationLevel.None,
            SanitizationLevel.Region,
            SanitizationLevel.RegionAndSpecialCharacters,
            SanitizationLevel.RegionAndSpecialCharactersAndRomanNumerals
        ];

        foreach (SanitizationLevel level in levels)
        {
            int matchCount = 0;

            string sanitizedSourceTitle = SanitizeName(sourceTitle, level);

            string sanitizedFoundTitle = SanitizeName(foundTitle, level);

            string[] parts = sanitizedFoundTitle.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (sanitizedSourceTitle.Contains(part))
                {
                    matchCount++;
                }
            }

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
        name = Path.GetFileNameWithoutExtension(name);

        string pattern = @"\s*\(.*?\)\s*";

        // Replace matches with an empty string
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
    /// Downloads media using the internal downloader.
    /// </summary>
    /// <param name="media">The media to download.</param>
    /// <returns>True if download succeeded.</returns>
    protected Task<bool> DownloadMedia(Media media)
    {
        return _downloader.DownloadMedia(media);
    }

    /// <summary>
    /// Sanitizes a game name by removing unwanted characters based on the specified level.
    /// </summary>
    /// <param name="name">The name to sanitize.</param>
    /// <param name="level">The level of sanitization to apply.</param>
    /// <returns>Sanitized name.</returns>
    protected string SanitizeName(string name, SanitizationLevel level)
    {
        if (level is SanitizationLevel.RegionAndSpecialCharacters or
            SanitizationLevel.RegionAndSpecialCharactersAndRomanNumerals)
        {
            name = Regex.Replace(name, @"[^a-zA-Z0-9\s]", "").Trim();
        }

        if (level is SanitizationLevel.Region or
            SanitizationLevel.RegionAndSpecialCharacters or
            SanitizationLevel.RegionAndSpecialCharactersAndRomanNumerals)
        {
            name = Regex.Replace(Regex.Replace(name, @"\s*\([^)]*\)$", ""), @"\s+", " ");
        }

        if (level == SanitizationLevel.RegionAndSpecialCharactersAndRomanNumerals)
        {
            name = ConvertRomanToNumbers(name);
            name = ConvertNumbersToRoman(name);
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
                var platformIdentifier = Configuration.PlatformTranslationConfigurations.FirstOrDefault(c => c.Platform == platform)?.Identifier;

                Logger.LogDebug("Scraping media for {Game} on platform {Platform} using {Source}", name, platform, Source);

                var context = await OnScrapeMedia(platformIdentifier, CleanName(name), previous);

                if (context != null)
                {
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

                    List<Task> downloadTasks = [];

                    List<Image> icons = context.Icons.ToList();

                    foreach (var media in context.Icons)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(icons, media));
                    }

                    List<Image> logos = context.Logos.ToList();

                    foreach (var media in context.Logos)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(logos, media));
                    }

                    List<Image> titles = context.Titles.ToList();

                    foreach (var media in context.Titles)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(titles, media));
                    }

                    List<Media> heros = context.Heros.ToList();

                    foreach (var media in context.Heros)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(heros, media));
                    }

                    List<Media> slides = context.Slides.ToList();

                    foreach (var media in context.Slides)
                    {
                        downloadTasks.Add(DownloadAndCheckMedia(slides, media));
                    }

                    await Task.WhenAll(downloadTasks);

                    context.Icons = icons;
                    context.Logos = logos;
                    context.Titles = titles;
                    context.Heros = heros;
                    context.Slides = slides;

                    Logger.LogDebug("Found {IconCount} icons, {LogoCount} logos, {TitleCount} titles, {HeroCount} heros, {SlideCount} slides from {Source}",
                        context.Icons.Count, context.Logos.Count, context.Titles.Count, context.Heros.Count, context.Slides.Count, Source);
                }

                HasScrapedGame = true;

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
    protected ScraperConfiguration Configuration
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
        /// <summary>No sanitization applied.</summary>
        None,
        /// <summary>Remove region info like (USA), (Europe).</summary>
        Region,
        /// <summary>Remove region info and special characters.</summary>
        RegionAndSpecialCharacters,
        /// <summary>Remove region info, special chars, and convert between Roman/Arabic numerals.</summary>
        RegionAndSpecialCharactersAndRomanNumerals,
    }

    /// <summary>
    /// Gets the source flag identifying this scraper.
    /// </summary>
    protected abstract SourceFlag Source { get; }

    /// <summary>
    /// Gets or sets whether this scraper has successfully scraped a game.
    /// Used to optimize subsequent scrapes for the same game.
    /// </summary>
    protected bool HasScrapedGame { get; set; }

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }
}
