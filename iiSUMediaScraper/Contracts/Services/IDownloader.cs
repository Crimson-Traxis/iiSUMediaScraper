using iiSUMediaScraper.Models;

namespace iiSUMediaScraper.Contracts.Services;

public interface IDownloader
{
    public event EventHandler<double> ProgressUpdated;

    /// <summary>
    /// Gets the folder path where external tools (yt-dlp.exe, ffmpeg.exe, ffprobe.exe, ffplay.exe) are located.
    /// </summary>
    string ToolsFolder { get; }

    Task<bool> DownloadImage(Image image, CancellationToken cancellationToken = default);

    Task<bool> DownloadVideo(Video video, CancellationToken cancellationToken = default);

    Task<bool> DownloadMusic(Music music, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets music tracks from a YouTube playlist URL using yt-dlp.
    /// Downloads thumbnails for each track but does not download the audio.
    /// </summary>
    /// <param name="playlistUrl">The YouTube playlist URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of Music objects from the playlist with downloaded thumbnails.</returns>
    Task<List<Music>> DownloadPlaylistDetails(string playlistUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads metadata and thumbnail for a music track without downloading the audio.
    /// Populates Title, Duration, LikeCount, and downloads the Thumbnail.
    /// </summary>
    /// <param name="music">The music object with a valid URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DownloadMusicDetails(Music music, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads metadata and thumbnail for a video without downloading the video file.
    /// Populates Title, Duration, LikeCount, and downloads the Thumbnail.
    /// </summary>
    /// <param name="video">The video object with a valid URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DownloadVideoDetails(Video video, CancellationToken cancellationToken = default);
}
