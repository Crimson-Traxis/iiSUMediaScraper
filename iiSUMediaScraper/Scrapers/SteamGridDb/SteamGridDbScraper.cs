using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Models.Scraping.SteamGridDb;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Text.Json;

namespace iiSUMediaScraper.Scrapers.SteamGridDb;

/// <summary>
/// Scraper for SteamGridDB, a database of game artwork including grids, logos, and heroes.
/// Supports fetching additional media on subsequent scrapes if initial searches came up empty.
/// </summary>
public class SteamGridDbScraper : Scraper
{
    private Game _game;

    private bool _hasFetchedLogos;

    private bool _hasFetchedTitles;

    private bool _hasFetchedHeros;

    /// <summary>
    /// Initializes a new instance of the SteamGridDbScraper.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public SteamGridDbScraper(IHttpClientFactory httpClientFactory, Configuration configuration, ILogger logger) : base(httpClientFactory, configuration, logger)
    {

    }

    /// <summary>
    /// Searches SteamGridDB for a game and fetches its media.
    /// On subsequent calls for the same game, can fetch additional media types if previous scrapes found none.
    /// </summary>
    /// <param name="name">Game name to search for.</param>
    /// <param name="previous">Previous scrape results to determine what additional media to fetch.</param>
    /// <returns>Game object with media, or null if not found.</returns>
    private async Task<Game?> ScrapeGame(string name, MediaContext? previous = null)
    {
        try
        {
            if (HasScrapedGame && _game != null)
            {
                List<Task> tasks = [];

                if (Configuration.IsFetchLogosIfNoneFound && !_hasFetchedLogos && previous?.Logos.Count == 0)
                {
                    tasks.Add(ScrapeLogos(_game.Id).ContinueWith(async t => _game.Logos = await t));
                }

                if (Configuration.IsFetchTitlesIfNoneFound && !_hasFetchedTitles && previous?.Titles.Count == 0)
                {
                    tasks.Add(ScrapeGrids(_game.Id).ContinueWith(async t => _game.Grids = await t));
                }

                if (Configuration.IsFetchHerosIfNoneFound && !_hasFetchedHeros && previous?.Heros.Count == 0)
                {
                    tasks.Add(ScrapeHero(_game.Id).ContinueWith(async t => _game.Heros = await t));
                }

                await Task.WhenAll(tasks);

                return _game;
            }
            else
            {
                HttpClient client = HttpClientFactory.CreateClient("SteamGridDb");

                Uri requestUri = new Uri($"https://www.steamgriddb.com/api/v2/search/autocomplete/{name}");

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
                {
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                };

                requestMessage.Headers.Add("Authorization", "Bearer 3cba3f74328cba0b55ee8b31c05d59b0");

                using HttpResponseMessage response = await client.SendAsync(requestMessage);

                response.EnsureSuccessStatusCode(); // Throws if the status code is an error

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var json = JsonSerializer.Deserialize<Response<Game>>(jsonResponse);

                if (json != null)
                {
                    foreach (var game in json.Data)
                    {
                        if (TitleMatches(CleanName(name), game.Name))
                        {
                            Logger.LogDebug("SteamGridDB: Found match for {Name}: {GameName}", name, game.Name);

                            List<Task> tasks = [];

                            if (Configuration.IsFetchLogos)
                            {
                                tasks.Add(ScrapeLogos(game.Id).ContinueWith(async t => game.Logos = await t));

                                _hasFetchedLogos = true;
                            }

                            if (Configuration.IsFetchTitles)
                            {
                                tasks.Add(ScrapeGrids(game.Id).ContinueWith(async t => game.Grids = await t));

                                _hasFetchedTitles = true;
                            }

                            if (Configuration.IsFetchHeros)
                            {
                                tasks.Add(ScrapeHero(game.Id).ContinueWith(async t => game.Heros = await t));

                                _hasFetchedHeros = true;
                            }

                            await Task.WhenAll(tasks);

                            return game;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SteamGridDB: Failed to scrape game {Name}", name);
        }

        return null;
    }

    /// <summary>
    /// Fetches hero/banner images for a specific game from SteamGridDB.
    /// Filters by configured hero styles if specified.
    /// </summary>
    /// <param name="gameId">SteamGridDB game ID.</param>
    /// <returns>Collection of hero images.</returns>
    private async Task<IEnumerable<Hero>> ScrapeHero(int gameId)
    {
        try
        {
            HttpClient client = HttpClientFactory.CreateClient("SteamGridDb");

            Uri requestUri = new Uri($"https://www.steamgriddb.com/api/v2/heroes/game/{gameId}");

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Add("Authorization", "Bearer 3cba3f74328cba0b55ee8b31c05d59b0");

            using HttpResponseMessage response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode(); // Throws if the status code is an error

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var json = JsonSerializer.Deserialize<Response<Hero>>(jsonResponse);

            if (json != null)
            {
                if (Configuration.HeroStyles.Count != 0)
                {
                    return json.Data.Where(d => Configuration.HeroStyles.Contains(d.Style));
                }

                return json.Data;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SteamGridDB: Failed to scrape heroes for game {GameId}", gameId);
        }

        return [];
    }

    /// <summary>
    /// Fetches grid/title images for a specific game from SteamGridDB.
    /// Filters by configured title styles if specified.
    /// </summary>
    /// <param name="gameId">SteamGridDB game ID.</param>
    /// <returns>Collection of grid images.</returns>
    private async Task<IEnumerable<Grid>> ScrapeGrids(int gameId)
    {
        try
        {
            HttpClient client = HttpClientFactory.CreateClient("SteamGridDb");

            Uri requestUri = new Uri($"https://www.steamgriddb.com/api/v2/grids/game/{gameId}");

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Add("Authorization", "Bearer 3cba3f74328cba0b55ee8b31c05d59b0");

            using HttpResponseMessage response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode(); // Throws if the status code is an error

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var json = JsonSerializer.Deserialize<Response<Grid>>(jsonResponse);

            if (json != null)
            {
                if (Configuration.TitleStyles.Count != 0)
                {
                    return json.Data.Where(d => Configuration.TitleStyles.Contains(d.Style));
                }

                return json.Data;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SteamGridDB: Failed to scrape grids for game {GameId}", gameId);
        }

        return [];
    }

    /// <summary>
    /// Fetches logo images for a specific game from SteamGridDB.
    /// Filters by configured logo styles if specified.
    /// </summary>
    /// <param name="gameId">SteamGridDB game ID.</param>
    /// <returns>Collection of logo images.</returns>
    private async Task<IEnumerable<Logo>> ScrapeLogos(int gameId)
    {
        try
        {
            HttpClient client = HttpClientFactory.CreateClient("SteamGridDb");

            Uri requestUri = new Uri($"https://www.steamgriddb.com/api/v2/logos/game/{gameId}");

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Add("Authorization", "Bearer 3cba3f74328cba0b55ee8b31c05d59b0");

            using HttpResponseMessage response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode(); // Throws if the status code is an error

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var json = JsonSerializer.Deserialize<Response<Logo>>(jsonResponse);

            if (json != null)
            {
                if (Configuration.LogoStyles.Count != 0)
                {
                    return json.Data.Where(d => Configuration.LogoStyles.Contains(d.Style));
                }

                return json.Data;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SteamGridDB: Failed to scrape logos for game {GameId}", gameId);
        }

        return [];
    }

    /// <summary>
    /// Scrapes media from SteamGridDB for the specified game.
    /// Tries multiple sanitization levels to find a match.
    /// </summary>
    /// <param name="platform">Platform identifier (not used by SteamGridDB).</param>
    /// <param name="name">Game name to search for.</param>
    /// <param name="previous">Previous scrape results.</param>
    /// <returns>Media context with all found media.</returns>
    protected override async Task<MediaContext?> OnScrapeMedia(string platform, string name, MediaContext? previous = null)
    {
        Game? game = await ScrapeGame(name, previous);

        game ??= await ScrapeGame(SanitizeName(name, SanitizationLevel.Region), previous);

        game ??= await ScrapeGame(SanitizeName(name, SanitizationLevel.RegionAndSpecialCharacters), previous);

        if (game != null)
        {
            _game = game;

            MediaContext mediaContext = new MediaContext();

            List<Image> heros = [];

            foreach (var hero in game.Heros)
            {
                heros.Add(new Image()
                {
                    Url = hero.Url,
                    Height = hero.Height,
                    Width = hero.Width,
                });
            }

            mediaContext.Heros = [.. heros.Cast<Media>()];

            List<Image> titles = [];

            foreach (var grid in game.Grids)
            {
                titles.Add(new Image()
                {
                    Url = grid.Url,
                    Height = grid.Height,
                    Width = grid.Width,
                });
            }

            mediaContext.Titles = titles;

            List<Image> logos = [];

            foreach (var logo in game.Logos)
            {
                logos.Add(new Image()
                {
                    Url = logo.Url,
                    Height = logo.Height,
                    Width = logo.Width
                });
            }

            mediaContext.Logos = logos;

            return mediaContext;
        }

        return null;
    }

    /// <summary>
    /// Gets the source flag for this scraper.
    /// </summary>
    protected override SourceFlag Source => SourceFlag.SteamGridDb;
}
