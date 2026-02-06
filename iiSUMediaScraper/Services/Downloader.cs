using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Scraping.Youtube;
using ImageMagick;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Downloads media files from URLs and populates their dimensions.
/// </summary>
public class Downloader : IDownloader
{
    /// <summary>
    /// Maximum number of retry attempts for failed downloads.
    /// </summary>
    private const int MaxRetries = 5;

    private long _lastProgressTicks;

    /// <summary>
    /// Raised when download progress is updated.
    /// </summary>
    public event EventHandler<double> ProgressUpdated;

    /// <summary>
    /// Initializes a new instance of the Downloader.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="uIThreadService">UI thread service for dispatching events.</param>
    /// <param name="fileService">File service for creating temporary files.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public Downloader(IHttpClientFactory httpClientFactory, IUIThreadService uIThreadService, IFileService fileService, IConfigurationService configurationService, ILogger<Downloader> logger)
    {
        HttpClientFactory = httpClientFactory;
        UIThreadService = uIThreadService;
        FileService = fileService;
        ConfigurationService = configurationService;
        Logger = logger;
        DownloadTasks = new ConcurrentDictionary<string, Task<string?>>();
    }

    /// <summary>
    /// Raises the ProgressUpdated event on the UI thread if the service is available.
    /// </summary>
    /// <param name="progress">Progress value (0-100).</param>
    private void RaiseProgressUpdated(double progress)
    {
        var now = Environment.TickCount64;
        if (progress < 100 && now - _lastProgressTicks < 1000)
            return;

        _lastProgressTicks = now;
        UIThreadService.DispachToUIThread(() =>
        {
            ProgressUpdated?.Invoke(this, progress);
        });
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
    /// Gets the appropriate download HTTP client name based on the source.
    /// </summary>
    /// <param name="source">The source flag indicating where the media originated.</param>
    /// <returns>The HTTP client name to use for downloads.</returns>
    private static string GetDownloadClientName(SourceFlag source)
    {
        return source switch
        {
            SourceFlag.Paste or SourceFlag.Local => "Download",
            _ => $"Download_{source}"
        };
    }

    /// <summary>
    /// Parses yt-dlp output line to extract download progress percentage.
    /// </summary>
    /// <param name="line">Output line from yt-dlp.</param>
    /// <returns>Progress percentage if found, null otherwise.</returns>
    private static double? ParseProgress(string line)
    {
        // yt-dlp outputs: [download]  10.5% of 100.00MiB at 1.00MiB/s ETA 00:09
        if (line.Contains("[download]") && line.Contains("%"))
        {
            var percentIndex = line.IndexOf('%');
            if (percentIndex > 0)
            {
                // Find the start of the number (work backwards from %)
                var startIndex = percentIndex - 1;
                while (startIndex >= 0 && (char.IsDigit(line[startIndex]) || line[startIndex] == '.'))
                {
                    startIndex--;
                }
                startIndex++;

                var percentStr = line.Substring(startIndex, percentIndex - startIndex);
                if (double.TryParse(percentStr, out var percent))
                {
                    return percent;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Executes yt-dlp with the specified arguments and waits for completion.
    /// Reports download progress via the ProgressUpdated event.
    /// </summary>
    /// <param name="arguments">Command line arguments for yt-dlp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if yt-dlp completed successfully.</returns>
    private async Task<bool> RunYtDlp(string arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(ConfigurationService.ToolsFolder, "yt-dlp.exe"),
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };

            process.Start();

            // Read stdout asynchronously to capture progress updates
            var outputTask = Task.Run(async () =>
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
                    if (line != null)
                    {
                        var progress = ParseProgress(line);
                        if (progress.HasValue)
                        {
                            RaiseProgressUpdated(progress.Value);
                        }
                    }
                }
            }, cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            await outputTask;

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                Logger.LogWarning("yt-dlp exited with code {ExitCode}: {Error}", process.ExitCode, error);

                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to run yt-dlp with arguments: {Arguments}", arguments);

            return false;
        }
    }

    /// <summary>
    /// Downloads content from YouTube and saves to a temporary file.
    /// Converts music to mp3 and videos to mp4 using ffmpeg.
    /// </summary>
    private async Task<string?> DownloadYoutubeToFile(string url, bool isVideo, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use framework's random file name generator (cryptographically strong)
            var tempFileName = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName());
            var tempDir = FileService.GetTemporaryFolder();

            // Ensure the temp folder exists
            System.IO.Directory.CreateDirectory(tempDir);

            // Use %(ext)s to let yt-dlp manage the final extension after conversion
            var outputTemplate = System.IO.Path.Combine(tempDir, $"{tempFileName}.%(ext)s");

            // Use yt-dlp to download and convert based on type
            // Video: use configured quality or default to "best", convert to mp4, limit to max duration if set
            // Music: best audio, extract and convert to mp3
            var videoQuality = ConfigurationService.Configuration?.VideoQuality ?? "best";
            var maxVideoDuration = ConfigurationService.Configuration?.MaxVideoDuration;
            var maxMusicDuration = ConfigurationService.Configuration?.MaxMusicDuration;
            var ffmpegPath = System.IO.Path.Combine(ConfigurationService.ToolsFolder, "ffmpeg.exe");
            var ffmpegExists = System.IO.File.Exists(ffmpegPath);

            if (!ffmpegExists)
            {
                Logger.LogWarning("ffmpeg not found at {Path}. Conversion to mp3/mp4 will be skipped.", ffmpegPath);
            }

            string formatArg;
            string conversionArg;
            string ffmpegLocationArg = ffmpegExists ? $"--ffmpeg-location \"{ffmpegPath}\"" : "";

            if (isVideo)
            {
                // Use configured quality with fallback to best available
                formatArg = $"-f \"{videoQuality}/best\"";
                conversionArg = ffmpegExists ? "--recode-video mp4" : "";
            }
            else
            {
                // Try best audio, then best overall with audio extraction
                formatArg = "-f \"bestaudio/best\"";
                conversionArg = ffmpegExists ? "--extract-audio --audio-format mp3" : "";
            }

            var maxDuration = isVideo ? maxVideoDuration : maxMusicDuration;
            var durationLimit = maxDuration.HasValue
                ? $"--download-sections \"*0-{(int)maxDuration.Value.TotalSeconds}\""
                : "";
            // --no-warnings: suppress informational warnings (like JS runtime)
            // --no-playlist: ensure we only download the single video, not a playlist
            var arguments = $"--no-warnings --no-playlist {formatArg} {conversionArg} {ffmpegLocationArg} {durationLimit} -o \"{outputTemplate}\" \"{url}\"";

            Logger.LogDebug("Downloading {Type} with yt-dlp: {Url} Args: {Args}", isVideo ? "Video" : "Music", url, arguments);

            var success = await RunYtDlp(arguments, cancellationToken);

            if (!success)
            {
                Logger.LogWarning("yt-dlp failed to download from {Url}", url);
                return null;
            }

            // Find the downloaded file - search for any file matching our GUID prefix
            // May find multiple files if conversion created intermediates
            var downloadedFiles = System.IO.Directory.GetFiles(tempDir, $"{tempFileName}.*");

            // Log all found files for debugging
            if (downloadedFiles.Length > 1)
            {
                Logger.LogDebug("Found {Count} files for {Url}: {Files}",
                    downloadedFiles.Length, url, string.Join(", ", downloadedFiles));
            }

            // Prefer the target format (mp3 for music, mp4 for video)
            var targetExtension = isVideo ? ".mp4" : ".mp3";
            var downloadedFile = downloadedFiles.FirstOrDefault(f => f.EndsWith(targetExtension, StringComparison.OrdinalIgnoreCase))
                                ?? downloadedFiles.FirstOrDefault();

            if (downloadedFile == null || !System.IO.File.Exists(downloadedFile))
            {
                Logger.LogWarning("yt-dlp download file not found for {Url}. Found {Count} files matching pattern: {Files}",
                    url, downloadedFiles.Length, string.Join(", ", downloadedFiles));
                return null;
            }

            // Verify the file has content
            var fileInfo = new System.IO.FileInfo(downloadedFile);
            if (fileInfo.Length == 0)
            {
                Logger.LogWarning("yt-dlp created empty file for {Url}: {Path}", url, downloadedFile);
                System.IO.File.Delete(downloadedFile);
                return null;
            }

            Logger.LogDebug("Downloaded from {Url} to {Path} ({Size} bytes)", url, downloadedFile, fileInfo.Length);

            return downloadedFile;
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("YouTube download cancelled for {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to download from {Url}", url);
            return null;
        }
    }

    /// <summary>
    /// Downloads content from YouTube using yt-dlp and updates the music object's LocalPath.
    /// Uses a concurrent dictionary to prevent duplicate downloads of the same URL.
    /// </summary>
    /// <param name="music">The music object containing the YouTube URL to download.</param>
    /// <param name="isVideo">True to download video+audio, false to download audio only.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the download was successful, false otherwise.</returns>
    private async Task<bool> DownloadYoutube(Music music, bool isVideo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(music.Url))
            return false;

        // Check if there's already a download in progress for this URL
        var downloadTask = DownloadTasks.GetOrAdd(music.Url, url => DownloadYoutubeToFile(url, isVideo, cancellationToken));

        var localPath = await downloadTask;

        if (localPath != null)
        {
            music.LocalPath = localPath;
            music.Extension = Path.GetExtension(localPath);
            return true;
        }

        // Remove failed task from cache so it can be retried
        DownloadTasks.TryRemove(music.Url, out _);
        return false;
    }

    /// <summary>
    /// Checks if the URL is a YouTube URL.
    /// </summary>
    private static bool IsYoutubeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            var uri = new Uri(url);
            var host = uri.Host.ToLowerInvariant();
            return host.Contains("youtube.com") || host.Contains("youtu.be");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Downloads a video from a direct URL to a temporary file.
    /// Retries up to 5 times on timeout or transient errors.
    /// Timeout is configured on the HttpClient in App.xaml.cs.
    /// </summary>
    /// <param name="url">The URL of the video to download.</param>
    /// <param name="source">The source flag indicating where the media originated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path to the downloaded temporary file, or null if download failed.</returns>
    private async Task<string?> DownloadVideoToFile(string url, SourceFlag source, CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var client = HttpClientFactory.CreateClient(GetDownloadClientName(source));

                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                // Check for transient HTTP errors that should be retried
                if (IsTransientError(response.StatusCode))
                {
                    lastException = new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.StatusCode}");
                    Logger.LogDebug("Transient error {StatusCode} downloading video on attempt {Attempt}/{MaxRetries}, retrying...",
                        (int)response.StatusCode, attempt + 1, MaxRetries);

                    await DelayBeforeRetry(attempt, cancellationToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var tempFilePath = await FileService.CreateTemporaryFile();

                var totalBytes = response.Content.Headers.ContentLength;
                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalRead += bytesRead;

                    if (totalBytes.HasValue && totalBytes.Value > 0)
                    {
                        var progress = (double)totalRead / totalBytes.Value * 100;
                        RaiseProgressUpdated(progress);
                    }
                }

                Logger.LogDebug("Downloaded video from {Url} to {Path}", url, tempFilePath);

                return tempFilePath;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // This was a timeout from HttpClient, not a user cancellation
                lastException = new TimeoutException("Download timed out", ex);
                Logger.LogDebug("Video download timeout on attempt {Attempt}/{MaxRetries} for {Url}, retrying...", attempt + 1, MaxRetries, url);

                await DelayBeforeRetry(attempt, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                Logger.LogDebug("Video download cancelled for {Url}", url);
                return null;
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                Logger.LogDebug(ex, "HTTP error downloading video on attempt {Attempt}/{MaxRetries} for {Url}, retrying...", attempt + 1, MaxRetries, url);

                await DelayBeforeRetry(attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to download video from {Url}", url);
                return null;
            }
        }

        Logger.LogWarning(lastException, "All {MaxRetries} retry attempts failed for video download from {Url}", MaxRetries, url);
        return null;
    }

    /// <summary>
    /// Downloads an image from a URL to a temporary file using HTTP GET.
    /// Reports download progress via the ProgressUpdated event.
    /// Retries up to 5 times on timeout or transient errors.
    /// Timeout is configured on the HttpClient in App.xaml.cs.
    /// </summary>
    /// <param name="url">The URL of the image to download.</param>
    /// <param name="source">The source flag indicating where the media originated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path to the downloaded temporary file, or null if download failed.</returns>
    private async Task<string?> DownloadImageToFile(string url, SourceFlag source, CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var client = HttpClientFactory.CreateClient(GetDownloadClientName(source));

                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                // Check for transient HTTP errors that should be retried
                if (IsTransientError(response.StatusCode))
                {
                    lastException = new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.StatusCode}");
                    Logger.LogDebug("Transient error {StatusCode} downloading image on attempt {Attempt}/{MaxRetries}, retrying...",
                        (int)response.StatusCode, attempt + 1, MaxRetries);

                    await DelayBeforeRetry(attempt, cancellationToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var tempFilePath = await FileService.CreateTemporaryFile();

                var totalBytes = response.Content.Headers.ContentLength;
                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalRead += bytesRead;

                    if (totalBytes.HasValue && totalBytes.Value > 0)
                    {
                        var progress = (double)totalRead / totalBytes.Value * 100;
                        RaiseProgressUpdated(progress);
                    }
                }

                Logger.LogDebug("Downloaded image from {Url} to {Path}", url, tempFilePath);

                return tempFilePath;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // This was a timeout from HttpClient, not a user cancellation
                lastException = new TimeoutException("Download timed out", ex);
                Logger.LogDebug("Image download timeout on attempt {Attempt}/{MaxRetries} for {Url}, retrying...", attempt + 1, MaxRetries, url);

                await DelayBeforeRetry(attempt, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                Logger.LogDebug("Image download cancelled for {Url}", url);
                return null;
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                Logger.LogDebug(ex, "HTTP error downloading image on attempt {Attempt}/{MaxRetries} for {Url}, retrying...", attempt + 1, MaxRetries, url);

                await DelayBeforeRetry(attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to download image from {Url}", url);
                return null;
            }
        }

        Logger.LogWarning(lastException, "All {MaxRetries} retry attempts failed for image download from {Url}", MaxRetries, url);
        return null;
    }

    /// <summary>
    /// Downloads an image from a URL and saves to a temporary file.
    /// Sets the LocalPath and dimensions on the image.
    /// </summary>
    /// <param name="image">The image to download. Must have a valid URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the download was successful, false otherwise.</returns>
    public Task<bool> DownloadImage(Image image, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            if (string.IsNullOrWhiteSpace(image.Url) || new Uri(image.Url).IsFile)
                return false;

            // Check if there's already a download in progress for this URL
            var downloadTask = DownloadTasks.GetOrAdd(image.Url, url => DownloadImageToFile(url, image.Source, cancellationToken));

            var localPath = await downloadTask;

            if (localPath != null && File.Exists(localPath))
            {
                image.LocalPath = localPath;
                // Get extension from URL since temp files have .tmp extension
                image.Extension = Path.GetExtension(new Uri(image.Url).AbsolutePath);

                // Get image dimensions
                try
                {
                    var imageInfo = new MagickImageInfo(localPath);
                    image.Width = (int)imageInfo.Width;
                    image.Height = (int)imageInfo.Height;

                    Logger.LogDebug("Image dimensions: {Url} ({Width}x{Height})", image.Url, image.Width, image.Height);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to get image dimensions for {Url}", image.Url);
                }

                return true;
            }

            // Remove failed task from cache so it can be retried
            DownloadTasks.TryRemove(image.Url, out _);
            return false;
        }, cancellationToken);
    }

    /// <summary>
    /// Downloads a video from a URL and saves to a temporary file.
    /// Uses yt-dlp for YouTube URLs or direct HTTP download for other URLs.
    /// Sets the LocalPath on the video object after successful download.
    /// </summary>
    /// <param name="video">The video to download. Must have a valid URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the download was successful, false otherwise.</returns>
    public Task<bool> DownloadVideo(Video video, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            if (string.IsNullOrWhiteSpace(video.Url))
                return false;

            // Use yt-dlp for YouTube URLs, direct download for others
            if (IsYoutubeUrl(video.Url))
            {
                return await DownloadYoutube(video, true, cancellationToken);
            }

            // Direct download for non-YouTube URLs
            if (new Uri(video.Url).IsFile)
                return false;

            var downloadTask = DownloadTasks.GetOrAdd(video.Url, url => DownloadVideoToFile(url, video.Source, cancellationToken));

            var localPath = await downloadTask;

            if (localPath != null && File.Exists(localPath))
            {
                video.LocalPath = localPath;
                // Get extension from URL since temp files have .tmp extension
                video.Extension = Path.GetExtension(new Uri(video.Url).AbsolutePath);
                return true;
            }

            // Remove failed task from cache so it can be retried
            DownloadTasks.TryRemove(video.Url, out _);
            return false;
        }, cancellationToken);
    }

    /// <summary>
    /// Downloads music from a YouTube URL using yt-dlp (audio only).
    /// Sets the LocalPath on the music object after successful download.
    /// </summary>
    /// <param name="music">The music to download. Must have a valid YouTube URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the download was successful, false otherwise.</returns>
    public Task<bool> DownloadMusic(Music music, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DownloadYoutube(music, false, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Executes yt-dlp with the specified arguments and returns the output.
    /// </summary>
    /// <param name="arguments">Command line arguments for yt-dlp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The standard output from yt-dlp, or null if execution failed.</returns>
    private async Task<string?> RunYtDlpWithOutput(string arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(ConfigurationService.ToolsFolder, "yt-dlp.exe"),
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
    /// Gets music tracks from a YouTube playlist URL using yt-dlp.
    /// Downloads thumbnails for each track in parallel but does not download the audio.
    /// </summary>
    /// <param name="playlistUrl">The YouTube playlist URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of Music objects from the playlist with downloaded thumbnails.</returns>
    public async Task<List<Music>> DownloadPlaylistDetails(string playlistUrl, CancellationToken cancellationToken = default)
    {
        var results = new List<Music>();

        if (string.IsNullOrWhiteSpace(playlistUrl))
            return results;

        // Single request to get all playlist entries with flat-playlist
        var output = await RunYtDlpWithOutput($"--dump-json --no-warnings --skip-download --flat-playlist \"{playlistUrl}\"", cancellationToken);

        if (string.IsNullOrWhiteSpace(output))
            return results;

        // Parse all entries first
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var youtubeVideo = JsonSerializer.Deserialize<YoutubeVideo>(line);

                if (youtubeVideo == null)
                    continue;

                // Build URL if not provided
                var musicUrl = youtubeVideo.Url;
                if (string.IsNullOrWhiteSpace(musicUrl) && !string.IsNullOrWhiteSpace(youtubeVideo.Id))
                {
                    musicUrl = $"https://www.youtube.com/watch?v={youtubeVideo.Id}";
                }

                if (!string.IsNullOrWhiteSpace(musicUrl))
                {
                    // Create thumbnail image (will download later in parallel)
                    Image? thumbnail = null;
                    if (!string.IsNullOrWhiteSpace(youtubeVideo.Id))
                    {
                        thumbnail = new Image { Url = $"https://i.ytimg.com/vi/{youtubeVideo.Id}/default.jpg" };
                    }

                    results.Add(new Music
                    {
                        Source = SourceFlag.Youtube,
                        Url = musicUrl,
                        Duration = youtubeVideo.Duration,
                        Title = youtubeVideo.Title,
                        LikeCount = youtubeVideo.LikeCount ?? 0,
                        Thumbnail = thumbnail
                    });

                    Logger.LogDebug("Found playlist music: {Title} ({Url})", youtubeVideo.Title, musicUrl);
                }
            }
            catch (JsonException ex)
            {
                Logger.LogDebug(ex, "Failed to parse yt-dlp music JSON line");
            }
        }

        // Download all thumbnails in parallel
        var thumbnailTasks = results
            .Where(m => m.Thumbnail != null)
            .Select(m => DownloadImage(m.Thumbnail!))
            .ToList();

        if (thumbnailTasks.Count > 0)
        {
            await Task.WhenAll(thumbnailTasks);
        }

        Logger.LogInformation("Found {Count} tracks in playlist: {Url}", results.Count, playlistUrl);

        return results;
    }

    /// <summary>
    /// Downloads metadata and thumbnail for a music track without downloading the audio.
    /// Populates Title, Duration, LikeCount, and downloads the Thumbnail.
    /// </summary>
    /// <param name="music">The music object with a valid URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public async Task<bool> DownloadMusicDetails(Music music, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(music.Url))
            return false;

        try
        {
            // Use flat-playlist for fast metadata retrieval (no format extraction)
            var output = await RunYtDlpWithOutput($"--dump-json --no-warnings --flat-playlist \"{music.Url}\"", cancellationToken);

            if (string.IsNullOrWhiteSpace(output))
                return false;

            // flat-playlist returns newline-separated JSON, take only the first entry
            var firstLine = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(firstLine))
                return false;

            var youtubeVideo = JsonSerializer.Deserialize<YoutubeVideo>(firstLine.Trim());

            if (youtubeVideo == null)
                return false;

            // Populate music details
            music.Title = youtubeVideo.Title;
            music.Duration = youtubeVideo.Duration;
            music.LikeCount = youtubeVideo.LikeCount ?? 0;
            music.Source = SourceFlag.Youtube;

            // Download thumbnail
            if (!string.IsNullOrWhiteSpace(youtubeVideo.Id))
            {
                music.Thumbnail = new Image { Url = $"https://i.ytimg.com/vi/{youtubeVideo.Id}/default.jpg" };
                await DownloadImage(music.Thumbnail, cancellationToken);
            }

            Logger.LogDebug("Downloaded music details: {Title} ({Url})", music.Title, music.Url);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to download music details for {Url}", music.Url);
            return false;
        }
    }

    /// <summary>
    /// Downloads metadata and thumbnail for a video without downloading the video file.
    /// Populates Title, Duration, LikeCount, and downloads the Thumbnail.
    /// </summary>
    /// <param name="video">The video object with a valid URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public async Task<bool> DownloadVideoDetails(Video video, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(video.Url))
            return false;

        try
        {
            // Use flat-playlist for fast metadata retrieval (no format extraction)
            var output = await RunYtDlpWithOutput($"--dump-json --no-warnings --flat-playlist \"{video.Url}\"", cancellationToken);

            if (string.IsNullOrWhiteSpace(output))
                return false;

            // flat-playlist returns newline-separated JSON, take only the first entry
            var firstLine = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(firstLine))
                return false;

            var youtubeVideo = JsonSerializer.Deserialize<YoutubeVideo>(firstLine.Trim());

            if (youtubeVideo == null)
                return false;

            // Populate video details
            video.Title = youtubeVideo.Title;
            video.Duration = youtubeVideo.Duration;
            video.LikeCount = youtubeVideo.LikeCount ?? 0;
            video.Source = SourceFlag.Youtube;

            // Download thumbnail
            if (!string.IsNullOrWhiteSpace(youtubeVideo.Id))
            {
                video.Thumbnail = new Image { Url = $"https://i.ytimg.com/vi/{youtubeVideo.Id}/default.jpg" };
                await DownloadImage(video.Thumbnail, cancellationToken);
            }

            Logger.LogDebug("Downloaded video details: {Title} ({Url})", video.Title, video.Url);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to download video details for {Url}", video.Url);
            return false;
        }
    }

    /// <summary>
    /// Gets the HTTP client factory for creating download clients.
    /// </summary>
    protected IHttpClientFactory HttpClientFactory { get; private set; }

    /// <summary>
    /// Gets the UI thread service for dispatching events.
    /// </summary>
    protected IUIThreadService UIThreadService { get; private set; }

    /// <summary>
    /// Gets the file service for creating temporary files.
    /// </summary>
    protected IFileService FileService { get; private set; }

    /// <summary>
    /// Gets the configuration service for accessing tool paths.
    /// </summary>
    protected IConfigurationService ConfigurationService { get; private set; }

    /// <summary>
    /// Gets the folder path where external tools (yt-dlp.exe, ffmpeg.exe, ffprobe.exe, ffplay.exe) are located.
    /// </summary>
    public string ToolsFolder => ConfigurationService.ToolsFolder;

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }

    /// <summary>
    /// Gets the dictionary of active download tasks keyed by URL to prevent duplicate downloads.
    /// </summary>
    protected ConcurrentDictionary<string, Task<string?>> DownloadTasks { get; private set; }
}
