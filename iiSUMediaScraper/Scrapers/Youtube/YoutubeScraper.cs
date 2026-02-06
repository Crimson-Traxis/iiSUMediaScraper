using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Models.Scraping.Youtube;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Web;

namespace iiSUMediaScraper.Scrapers.Youtube;

public class YoutubeScraper : Scraper
{
    /// <summary>
    /// Initializes a new instance of the YoutubeScraper.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="mediaCache">Shared media cache for this scraping session.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public YoutubeScraper(IHttpClientFactory httpClientFactory, IDownloader downloader, Configuration configuration, ILogger logger) : base(httpClientFactory, downloader, configuration, logger)
    {

    }

    /// <summary>
    /// Executes yt-dlp with the specified arguments and returns the output.
    /// </summary>
    /// <param name="arguments">Command line arguments for yt-dlp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The standard output from yt-dlp.</returns>
    private async Task<string?> RunYtDlp(string arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(ToolsFolder, "yt-dlp.exe"),
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                Logger.LogWarning("yt-dlp exited with code {ExitCode}: {Error}", process.ExitCode, error);

                return null;
            }

            return output;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to run yt-dlp with arguments: {Arguments}", arguments);

            return null;
        }
    }

    /// <summary>
    /// Searches YouTube for playlists matching the search term using yt-dlp.
    /// Uses YouTube's search URL with playlist filter (sp=EgIQAw%3D%3D).
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="maxResults">Maximum number of playlist results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of playlist results.</returns>
    private async Task<List<YoutubePlaylist>> SearchPlaylists(string searchTerm, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var results = new List<YoutubePlaylist>();

        // Use YouTube search URL with playlist filter (sp=EgIQAw%3D%3D filters to playlists only)
        var encodedQuery = HttpUtility.UrlEncode(searchTerm);
        var searchUrl = $"https://www.youtube.com/results?search_query={encodedQuery}&sp=EgIQAw%3D%3D";

        // Use --dump-single-json to get the search results structure with entries array
        var output = await RunYtDlp($"--dump-single-json --flat-playlist --no-warnings --playlist-end {maxResults} \"{searchUrl}\"", cancellationToken);

        if (string.IsNullOrWhiteSpace(output))
            return results;

        try
        {
            var searchResult = JsonSerializer.Deserialize<YoutubeSearchResult>(output);

            if (searchResult?.Entries == null)
                return results;

            foreach (var entry in searchResult.Entries)
            {
                // Playlists in search results have ie_key "YoutubeTab" or _type "playlist"
                if (entry.Type == "playlist" || entry.IeKey == "YoutubeTab" || entry.IeKey == "YoutubePlaylist")
                {
                    // Build playlist URL if not provided
                    if (string.IsNullOrWhiteSpace(entry.Url) && !string.IsNullOrWhiteSpace(entry.Id))
                    {
                        entry.Url = $"https://www.youtube.com/playlist?list={entry.Id}";
                    }

                    if (!string.IsNullOrWhiteSpace(entry.Title) && !string.IsNullOrWhiteSpace(entry.Url))
                    {
                        // Avoid duplicates
                        if (!results.Any(p => p.Id == entry.Id))
                        {
                            results.Add(entry);
                            Logger.LogDebug("Youtube: Found playlist in search: {Title} ({Url})", entry.Title, entry.Url);
                        }
                    }
                }

                if (results.Count >= maxResults)
                    break;
            }
        }
        catch (JsonException ex)
        {
            Logger.LogDebug(ex, "Failed to parse yt-dlp search results JSON");
        }

        return results;
    }

    /// <summary>
    /// Gets videos from a playlist using yt-dlp. When IsLoadVideoDetails is enabled,
    /// fetches full metadata including like counts; otherwise uses flat-playlist for faster results.
    /// </summary>
    /// <param name="playlistUrl">The playlist URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of video results.</returns>
    private async Task<List<YoutubeVideo>> GetPlaylistVideos(string playlistUrl, CancellationToken cancellationToken = default)
    {
        var results = new List<YoutubeVideo>();

        // Use --flat-playlist for faster results when video details aren't needed
        // Without --flat-playlist, we get full metadata including like_count
        var flatPlaylistArg = GlobalConfiguration.IsSortMusicByLikes ? "" : "--flat-playlist ";
        var matchFilter = GlobalConfiguration.SearchMusicMaxDuration.HasValue
            ? $"--match-filter \"duration<={(int)GlobalConfiguration.SearchMusicMaxDuration.Value.TotalSeconds}\" "
            : "";
        var output = await RunYtDlp($"--dump-json --no-warnings --skip-download {matchFilter}{flatPlaylistArg}\"{playlistUrl}\"", cancellationToken);

        if (string.IsNullOrWhiteSpace(output))
            return results;

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var video = JsonSerializer.Deserialize<YoutubeVideo>(line);

                if (video == null)
                    continue;

                // Build URL if not provided
                if (string.IsNullOrWhiteSpace(video.Url) && !string.IsNullOrWhiteSpace(video.Id))
                {
                    video.Url = $"https://www.youtube.com/watch?v={video.Id}";
                }

                if (!string.IsNullOrWhiteSpace(video.Url))
                {
                    results.Add(video);
                }
            }
            catch (JsonException ex)
            {
                Logger.LogDebug(ex, "Failed to parse yt-dlp video JSON line");
            }
        }

        return results;
    }

    private async Task<IEnumerable<Music>?> ScrapeMusic(string platform, string name, MediaContext? previous = null)
    {
        try
        {
            if (!HasScraped && Configuration.IsFetchMusic)
            {
                var platformName = GlobalConfiguration.PlatformConfigurations.FirstOrDefault(c => c.Code == platform)?.Name ?? platform;
                var searchTerm = $"{platformName} {name} OST";

                Logger.LogDebug("Youtube: Searching for playlists with term: {SearchTerm}", searchTerm);

                // Only get the first playlist - assume it's the correct one
                var playlists = await SearchPlaylists(searchTerm, maxResults: 1);
                var playlist = playlists.FirstOrDefault();

                if (playlist == null)
                    return null;

                Logger.LogDebug("Youtube: Found matching playlist: {PlaylistTitle}", playlist.Title);

                var videos = await GetPlaylistVideos(playlist.Url!);

                if (videos.Count == 0)
                    return null;

                // Sort by like count descending (most liked first)
                // Construct small thumbnail URL from video ID (default.jpg = 120x90)
                var maxSearchDuration = GlobalConfiguration.SearchMusicMaxDuration;

                return videos
                    .Where(v => !string.IsNullOrEmpty(v.Id))
                    .Where(v => !maxSearchDuration.HasValue || v.Duration <= maxSearchDuration.Value)
                    .Select(v => new Music()
                    {
                        Source = SourceFlag.Youtube,
                        Url = v.Url ?? $"https://www.youtube.com/watch?v={v.Id}",
                        Duration = v.Duration,
                        LikeCount = v.LikeCount ?? 0,
                        Title = v.Title,
                        Thumbnail = new Image()
                        {
                            Url = $"https://i.ytimg.com/vi/{v.Id}/default.jpg"
                        }
                    });
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Youtube: Failed to scrape music {Name}", name);
        }

        return null;
    }

    protected override async Task<MediaContext?> OnScrapeMedia(string platform, string name, MediaContext? previous = null)
    {
        IEnumerable<Music>? music = await ScrapeMusic(platform, name, previous);

        music ??= await ScrapeMusic(platform, SanitizeName(name, SanitizationLevel.Region), previous);

        music ??= await ScrapeMusic(platform, SanitizeName(name, SanitizationLevel.RegionAndSpecialCharacters), previous);

        if (music != null)
        {
            return new MediaContext()
            {
                Music = [.. music]
            };
        }

        return null;
    }

    /// <summary>
    /// Gets the source flag for this scraper.
    /// </summary>
    protected override SourceFlag Source => SourceFlag.Youtube;
}
