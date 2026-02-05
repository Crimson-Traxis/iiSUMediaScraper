using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Models.Scraping.Igdb;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;

namespace iiSUMediaScraper.Scrapers.Igdb;

/// <summary>
/// Scraper for IGDB (Internet Game Database), accessed via Twitch OAuth.
/// Fetches game covers, screenshots, and videos. Supports alternate name matching.
/// </summary>
public class IgdbScraper : Scraper
{
    private static volatile TokenResponse? _token;

    private Game _game;

    private readonly bool _hasFetchedTitles;

    private readonly bool _hasFetchedSlides;

    private readonly bool _hasFetchedVideos;

    /// <summary>
    /// Initializes a new instance of the IgdbScraper.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="mediaCache">Shared media cache for this scraping session.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public IgdbScraper(IHttpClientFactory httpClientFactory, IDownloader downlaoder, Configuration configuration, ILogger logger) : base(httpClientFactory, downlaoder, configuration, logger)
    {

    }

    /// <summary>
    /// Generates or refreshes the OAuth access token from Twitch for IGDB API access.
    /// Token is cached and reused until it expires.
    /// </summary>
    /// <returns>True if token generation succeeded, false otherwise.</returns>
    private async Task<bool> GenerateToken()
    {
        try
        {
            if (!string.IsNullOrEmpty(GlobalConfiguration.IgdbClientId) && !string.IsNullOrEmpty(GlobalConfiguration.IgdbClientSecret) && _token == null || (_token?.ExpiresOn <= DateTime.Now))
            {
                HttpClient client = HttpClientFactory.CreateClient("Igdb");

                var requestUri = new Uri($"https://id.twitch.tv/oauth2/token?client_id={GlobalConfiguration.IgdbClientId}&client_secret={GlobalConfiguration.IgdbClientSecret}&grant_type=client_credentials");

                using HttpResponseMessage response = await SendWithRetryAsync(
                    client,
                    () => new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = new StringContent("", Encoding.UTF8, "application/json")
                    });

                response.EnsureSuccessStatusCode(); // Throws if the status code is an error

                string jsonResponse = await response.Content.ReadAsStringAsync();

                _token = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

                if (_token != null)
                {
                    _token.ExpiresOn = DateTime.Now.AddSeconds(_token.ExpiresIn).AddHours(1);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "IGDB: Failed to generate OAuth token");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Searches IGDB for a game and fetches its media.
    /// On subsequent calls for the same game, can fetch additional media types if previous scrapes found none.
    /// </summary>
    /// <param name="platformId">IGDB platform ID to filter results.</param>
    /// <param name="name">Game name to search for.</param>
    /// <param name="previous">Previous scrape results to determine what additional media to fetch.</param>
    /// <returns>Game object with media, or null if not found.</returns>
    private async Task<Game?> ScrapeGame(int platformId, string name, MediaContext? previous = null)
    {
        try
        {
            if (HasScraped && _game != null)
            {
                List<Task> tasks = [];

                if (Configuration.IsFetchTitlesIfNoneFound && !_hasFetchedTitles && previous?.Titles.Count == 0)
                {
                    tasks.Add(ScrapeCover(_game.Id).ContinueWith(async t => _game.Cover = await t));
                }

                if (Configuration.IsFetchSlidesIfNoneFound && !_hasFetchedSlides && previous?.Slides.Count == 0)
                {
                    tasks.Add(ScrapeScreenshots(_game.Id).ContinueWith(async t => _game.Screenshots = await t));
                }

                if (Configuration.IsFetchVideos && !_hasFetchedVideos && previous?.Videos.Count() == 0)
                {
                    tasks.Add(ScrapeVideos(_game).ContinueWith(async t => _game.Videos = await t));
                }

                await Task.WhenAll(tasks);

                return _game;
            }
            else if (await GenerateToken() && _token != null)
            {
                HttpClient client = HttpClientFactory.CreateClient("Igdb");

                var requestUri = new Uri($"https://api.igdb.com/v4/games");

                string body = @$"fields *;
                            search ""{name}"";
                            limit 500;";

                using HttpResponseMessage response = await SendWithRetryAsync(
                    client,
                    () =>
                    {
                        var msg = new HttpRequestMessage(HttpMethod.Post, requestUri)
                        {
                            Content = new StringContent(body, Encoding.UTF8, "application/json")
                        };
                        msg.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");
                        msg.Headers.Add("Client-ID", GlobalConfiguration.IgdbClientId);
                        return msg;
                    });

                response.EnsureSuccessStatusCode(); // Throws if the status code is an error

                string jsonResponse = await response.Content.ReadAsStringAsync();

                IEnumerable<Game>? json = JsonSerializer.Deserialize<IEnumerable<Game>>(jsonResponse);

                if (json != null)
                {
                    foreach (Game? game in json.Where(g => g.Platforms.Any(p => p.Id == platformId)))
                    {
                        bool foundMatch = TitleMatches(CleanName(name), game.Name);

                        if (!foundMatch)
                        {
                            foreach (AlternativeName alternate in await ScrapeAlternateNames(game.Id))
                            {
                                foundMatch |= TitleMatches(CleanName(name), alternate.Name);
                            }
                        }

                        if (foundMatch)
                        {
                            Logger.LogDebug("IGDB: Found match for {Name}: {GameName}", name, game.Name);

                            List<Task> tasks = [];

                            if (Configuration.IsFetchTitles)
                            {
                                tasks.Add(ScrapeCover(game.Id).ContinueWith(async t => game.Cover = await t));
                            }

                            if (Configuration.IsFetchSlides)
                            {
                                tasks.Add(ScrapeScreenshots(game.Id).ContinueWith(async t => game.Screenshots = await t));
                            }

                            if (Configuration.IsFetchVideos)
                            {
                                tasks.Add(ScrapeVideos(game).ContinueWith(async t => game.Videos = await t));
                            }

                            await Task.WhenAll(tasks);

                            _game = game;

                            return game;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "IGDB: Failed to scrape game {Name}", name);
        }

        return null;
    }

    /// <summary>
    /// Fetches alternative names for a game from IGDB.
    /// Used to improve title matching for games with regional or alternate names.
    /// </summary>
    /// <param name="gameId">IGDB game ID.</param>
    /// <returns>Collection of alternative names.</returns>
    private async Task<IEnumerable<AlternativeName>> ScrapeAlternateNames(int gameId)
    {
        try
        {
            if (await GenerateToken() && _token != null)
            {
                HttpClient client = HttpClientFactory.CreateClient("Igdb");

                var requestUri = new Uri($"https://api.igdb.com/v4/alternative_names");

                string body = @$"fields *;
                                where game = {gameId};";

                using HttpResponseMessage response = await SendWithRetryAsync(
                    client,
                    () =>
                    {
                        var msg = new HttpRequestMessage(HttpMethod.Post, requestUri)
                        {
                            Content = new StringContent(body, Encoding.UTF8, "application/json")
                        };
                        msg.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");
                        msg.Headers.Add("Client-ID", GlobalConfiguration.IgdbClientId);
                        return msg;
                    });

                response.EnsureSuccessStatusCode(); // Throws if the status code is an error

                string jsonResponse = await response.Content.ReadAsStringAsync();

                IEnumerable<AlternativeName>? json = JsonSerializer.Deserialize<IEnumerable<AlternativeName>>(jsonResponse);

                if (json != null)
                {
                    return json;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "IGDB: Failed to scrape alternate names for game {GameId}", gameId);
        }

        return [];
    }

    /// <summary>
    /// Fetches video URLs for a specific game from IGDB.
    /// Converts IGDB video IDs to YouTube URLs.
    /// </summary>
    /// <param name="gameId">IGDB game ID.</param>
    /// <returns>Collection of video URLs.</returns>
    private async Task<IEnumerable<IgdbVideo>> ScrapeVideos(Game game)
    {
        try
        {
            if (await GenerateToken() && _token != null && game.Videos.Count() > 0)
            {
                HttpClient client = HttpClientFactory.CreateClient("Igdb");

                var requestUri = new Uri($"https://api.igdb.com/v4/game_videos");

                string body = @$"fields *;
                                where game = {game.Id};";

                using HttpResponseMessage response = await SendWithRetryAsync(
                    client,
                    () =>
                    {
                        var msg = new HttpRequestMessage(HttpMethod.Post, requestUri)
                        {
                            Content = new StringContent(body, Encoding.UTF8, "application/json")
                        };
                        msg.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");
                        msg.Headers.Add("Client-ID", GlobalConfiguration.IgdbClientId);
                        return msg;
                    });

                response.EnsureSuccessStatusCode(); // Throws if the status code is an error

                string jsonResponse = await response.Content.ReadAsStringAsync();

                IEnumerable<IgdbVideo>? json = JsonSerializer.Deserialize<IEnumerable<IgdbVideo>>(jsonResponse);

                if (json != null)
                {
                    foreach (IgdbVideo video in json)
                    {
                        video.Url = "https://youtube.com/watch?v=" + video.Url;
                    }

                    return json;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "IGDB: Failed to scrape videos for game {GameId}", game.Id);
        }

        return [];
    }

    /// <summary>
    /// Fetches the cover image for a specific game from IGDB.
    /// Converts thumbnail URLs to high-resolution cover images.
    /// </summary>
    /// <param name="gameId">IGDB game ID.</param>
    /// <returns>Cover image, or null if not found.</returns>
    private async Task<Cover?> ScrapeCover(int gameId)
    {
        try
        {
            if (await GenerateToken() && _token != null)
            {
                HttpClient client = HttpClientFactory.CreateClient("Igdb");

                var requestUri = new Uri($"https://api.igdb.com/v4/covers");

                string body = @$"fields *;
                                where game = {gameId};";

                using HttpResponseMessage response = await SendWithRetryAsync(
                    client,
                    () =>
                    {
                        var msg = new HttpRequestMessage(HttpMethod.Post, requestUri)
                        {
                            Content = new StringContent(body, Encoding.UTF8, "application/json")
                        };
                        msg.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");
                        msg.Headers.Add("Client-ID", GlobalConfiguration.IgdbClientId);
                        return msg;
                    });

                response.EnsureSuccessStatusCode(); // Throws if the status code is an error

                string jsonResponse = await response.Content.ReadAsStringAsync();

                IEnumerable<Cover>? json = JsonSerializer.Deserialize<IEnumerable<Cover>>(jsonResponse);

                if (json != null)
                {
                    foreach (Cover cover in json)
                    {
                        cover.Url = cover.Url.Replace("//images.igdb.com/igdb/image/upload/t_thumb", "https://images.igdb.com/igdb/image/upload/t_cover_big");
                    }

                    if (json != null)
                    {
                        return json.FirstOrDefault();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "IGDB: Failed to scrape cover for game {GameId}", gameId);
        }

        return null;
    }

    /// <summary>
    /// Fetches screenshot images for a specific game from IGDB.
    /// Converts thumbnail URLs to 1080p resolution screenshots.
    /// </summary>
    /// <param name="gameId">IGDB game ID.</param>
    /// <returns>Collection of screenshot images.</returns>
    private async Task<IEnumerable<Screenshot>> ScrapeScreenshots(int gameId)
    {
        try
        {
            if (await GenerateToken() && _token != null)
            {
                HttpClient client = HttpClientFactory.CreateClient("Igdb");

                var requestUri = new Uri($"https://api.igdb.com/v4/screenshots");

                string body = @$"fields *;
                                where game = {gameId};";

                using HttpResponseMessage response = await SendWithRetryAsync(
                    client,
                    () =>
                    {
                        var msg = new HttpRequestMessage(HttpMethod.Post, requestUri)
                        {
                            Content = new StringContent(body, Encoding.UTF8, "application/json")
                        };
                        msg.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");
                        msg.Headers.Add("Client-ID", GlobalConfiguration.IgdbClientId);
                        return msg;
                    });

                response.EnsureSuccessStatusCode(); // Throws if the status code is an error

                string jsonResponse = await response.Content.ReadAsStringAsync();

                IEnumerable<Screenshot>? json = JsonSerializer.Deserialize<IEnumerable<Screenshot>>(jsonResponse);

                if (json != null)
                {
                    foreach (Screenshot cover in json)
                    {
                        cover.Url = cover.Url.Replace("//images.igdb.com/igdb/image/upload/t_thumb", "https://images.igdb.com/igdb/image/upload/t_1080p");
                    }

                    return json;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "IGDB: Failed to scrape screenshots for game {GameId}", gameId);
        }

        return [];
    }

    /// <summary>
    /// Scrapes media from IGDB for the specified game.
    /// Tries multiple sanitization levels to find a match.
    /// </summary>
    /// <param name="platform">IGDB platform ID as string.</param>
    /// <param name="name">Game name to search for.</param>
    /// <param name="previous">Previous scrape results.</param>
    /// <returns>Media context with all found media.</returns>
    protected override async Task<MediaContext?> OnScrapeMedia(string platform, string name, MediaContext? previous = null)
    {
        if (!int.TryParse(platform, out int platformId))
        {
            platformId = -1;
        }

        Game? game = await ScrapeGame(platformId, name, previous);

        game ??= await ScrapeGame(platformId, SanitizeName(name, SanitizationLevel.Region), previous);

        game ??= await ScrapeGame(platformId, SanitizeName(name, SanitizationLevel.RegionAndSpecialCharacters), previous);

        if (game != null)
        {
            _game = game;

            var mediaContext = new MediaContext();

            if (game.Cover != null)
            {
                mediaContext.Titles = [new Image()
                    {
                        Url = game.Cover.Url,
                        Width = game.Cover.Width,
                        Height = game.Cover.Height,
                    }];
            }

            List<Image> slides = [];

            foreach (Screenshot screenshot in game.Screenshots)
            {
                slides.Add(new Image()
                {
                    Url = screenshot.Url,
                    Width = screenshot.Width,
                    Height = screenshot.Height
                });
            }

            mediaContext.Slides = slides;

            if(Configuration.IsFetchVideos)
            {
                List<Video> videos = [];

                foreach (IgdbVideo video in game.Videos)
                {
                    videos.Add(new Models.Video()
                    {
                        Url = video.Url,
                        ApplyMediaType = MediaType.Slide,
                        Title = video.Name,
                    });
                }

                mediaContext.Videos = videos;
            }

            return mediaContext;
        }

        return null;
    }

    protected override SourceFlag Source => SourceFlag.Igdb;
}
