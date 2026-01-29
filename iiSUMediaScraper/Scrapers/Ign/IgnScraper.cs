using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Models.Scraping.Ign;
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace iiSUMediaScraper.Scrapers.Ign;

/// <summary>
/// Scraper for IGN (Imagine Games Network), a gaming media website.
/// Fetches game artwork and screenshots from IGN's GraphQL API and website.
/// </summary>
public class IgnScraper : Scraper
{
    private Game _game;

    /// <summary>
    /// Initializes a new instance of the IgnScraper.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public IgnScraper(IHttpClientFactory httpClientFactory, Configuration configuration, ILogger logger) : base(httpClientFactory, configuration, logger)
    {

    }

    /// <summary>
    /// Searches IGN for a game using their GraphQL API.
    /// Matches games by platform and title, checking alternate names if needed.
    /// </summary>
    /// <param name="platformId">IGN platform ID to filter results.</param>
    /// <param name="name">Game name to search for.</param>
    /// <returns>Game object with media, or null if not found.</returns>
    private async Task<Game?> ScrapeGame(int platformId, string name)
    {
        try
        {
            HttpClient client = HttpClientFactory.CreateClient("Ign");

            string variables = "{ \"term\":\"" + name + "\", \"count\":200, \"objectType\":\"Game\" }";

            string extensions = "{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"e1c2e012a21b4a98aaa618ef1b43eb0cafe9136303274a34f5d9ea4f2446e884\"}}";

            Uri requestUri = new Uri($"https://mollusk.apis.ign.com/graphql?operationName=SearchObjectsByName&variables={variables}&extensions={extensions}");

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode(); // Throws if the status code is an error

            string jsonResponse = await response.Content.ReadAsStringAsync();

            Response<SearchGameResult>? json = JsonSerializer.Deserialize<Response<SearchGameResult>>(jsonResponse);

            if (json != null && json.Data != null && json.Data.Result != null)
            {
                foreach (Game? game in json.Data.Result.Games.Where(g => g.Regions.Any(r => r.Releases.Any(r => r.PlatformAttributes.Any(p => p.Id == platformId)))))
                {
                    bool foundMatch = TitleMatches(CleanName(name), game.Metadata.Names.Name);

                    if (!foundMatch)
                    {
                        foreach (var alternate in game.Metadata.Names.Alternates)
                        {
                            foundMatch |= TitleMatches(CleanName(name), alternate);
                        }
                    }

                    if (foundMatch)
                    {
                        Logger.LogDebug("IGN: Found match for {Name}: {GameName}", name, game.Metadata.Names.Name);

                        if (Configuration.IsFetchSlides)
                        {
                            game.Images = await ScrapeScreenshots(game.Slug);
                        }

                        return game;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "IGN: Failed to scrape game {Name}", name);
        }

        return null;
    }

    /// <summary>
    /// Scrapes screenshot images from an IGN game page.
    /// Parses HTML from the game page and extracts image URLs using regex.
    /// </summary>
    /// <param name="game">Game slug (URL-friendly identifier).</param>
    /// <returns>Collection of screenshot images.</returns>
    private async Task<IEnumerable<Screenshot>> ScrapeScreenshots(string game)
    {
        List<Screenshot> images = [];

        try
        {
            HttpClient client = HttpClientFactory.CreateClient("Ign");

            Uri requestUri = new Uri($"https://www.ign.com/games/{game}");

            using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            requestMessage.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            requestMessage.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            requestMessage.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            requestMessage.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            using HttpResponseMessage response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode(); // Throws if the status code is an error

            Stream htmlStream = await response.Content.ReadAsStreamAsync();
            byte[] htmlBits;
            string html = "";

            using (MemoryStream memoryStream = new MemoryStream())
            {
                htmlStream.CopyTo(memoryStream);

                htmlBits = memoryStream.ToArray();
            }

            using (MemoryStream compressedStream = new MemoryStream(htmlBits))
            using (GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (MemoryStream resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);

                resultStream.Position = 0;
                using StreamReader streamReader = new StreamReader(resultStream);
                html = streamReader.ReadToEnd();
            }

            Regex regex = new Regex(@"src=""(https://assets[^""]*)""", RegexOptions.IgnoreCase);

            MatchCollection matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                string? url = match.Groups[1].Value.Split("?").FirstOrDefault();

                if (url != null)
                {
                    images.Add(new Screenshot()
                    {
                        Url = url
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "IGN: Failed to scrape screenshots for {Game}", game);
        }

        return images;
    }

    /// <summary>
    /// Scrapes media from IGN for the specified game.
    /// Tries multiple sanitization levels to find a match.
    /// Can fetch additional media on subsequent calls if previous scrapes found none.
    /// </summary>
    /// <param name="platform">IGN platform ID as string.</param>
    /// <param name="name">Game name to search for.</param>
    /// <param name="previous">Previous scrape results.</param>
    /// <returns>Media context with all found media.</returns>
    protected override async Task<MediaContext?> OnScrapeMedia(string platform, string name, MediaContext? previous = null)
    {
        if (!int.TryParse(platform, out int platformId))
        {
            platformId = -1;
        }

        if (HasScrapedGame && _game != null)
        {
            var mediaContext = new MediaContext();

            if (Configuration.IsFetchIconsIfNoneFound && previous?.Icons.Count == 0)
            {
                mediaContext.Icons = [new Models.Image()
                {
                    Url = _game.PrimaryImage.Url
                }];
            }

            if (Configuration.IsFetchTitlesIfNoneFound && previous?.Titles.Count == 0)
            {
                mediaContext.Titles = [new Models.Image()
                {
                    Url = _game.PrimaryImage.Url
                }];
            }

            return mediaContext;
        }
        else
        {
            Game? game = await ScrapeGame(platformId, name);

            game ??= await ScrapeGame(platformId, SanitizeName(name, SanitizationLevel.Region));

            game ??= await ScrapeGame(platformId, SanitizeName(name, SanitizationLevel.RegionAndSpecialCharacters));

            if (game != null)
            {
                _game = game;

                var mediaContext = new MediaContext();

                if (Configuration.IsFetchIcons)
                {
                    mediaContext.Icons = [new Models.Image()
                    {
                        Url = game.PrimaryImage.Url
                    }];
                }

                if (Configuration.IsFetchTitles)
                {
                    mediaContext.Titles = [new Models.Image()
                    {
                        Url = game.PrimaryImage.Url
                    }];
                }

                return mediaContext;
            }
        }

        return null;
    }

    protected override SourceFlag Source => SourceFlag.Ign;
}
