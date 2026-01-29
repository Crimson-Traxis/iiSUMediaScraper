using iiSUMediaScraper.Models;
using ImageMagick;
using Microsoft.Extensions.Logging;

namespace iiSUMediaScraper.Scrapers;

/// <summary>
/// Downloads media files from URLs and populates their dimensions.
/// </summary>
public class Downloader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the Downloader.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public Downloader(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Downloads media from a URL and populates its byte data and dimensions.
    /// For images, uses ImageMagick to determine width and height.
    /// Video downloading is not currently supported.
    /// </summary>
    /// <param name="media">The media to download. Must have a valid URL and empty Bytes array.</param>
    /// <returns>True if the download was successful and bytes were populated, false otherwise.</returns>
    public async Task<bool> DownloadMedia(Media media)
    {
        if (media is Image image &&
            image.Bytes.Length == 0 &&
            !string.IsNullOrWhiteSpace(image.Url) &&
            !new Uri(image.Url).IsFile)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("Download");

                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

                media.Bytes = await client.GetByteArrayAsync(media.Url);

                MagickImageInfo imageInfo = new MagickImageInfo(media.Bytes);

                image.Width = (int)imageInfo.Width;

                image.Height = (int)imageInfo.Height;

                _logger.LogDebug("Downloaded image from {Url} ({Width}x{Height})", media.Url, image.Width, image.Height);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download media from {Url}", media.Url);
                return false;
            }

            return media.Bytes.Length != 0;
        }
        else if (media is Video video)
        {
            // Video downloading not supported atm
            _logger.LogDebug("Video downloading not supported: {Url}", video.Url);
            return false;
        }

        return false;
    }
}
