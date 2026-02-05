namespace iiSUMediaScraper.Models;

/// <summary>
/// Represents a music/audio media item.
/// </summary>
public class Music : Media
{
    /// <summary>
    /// Gets or sets the duration of the music track.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the like count from the source platform.
    /// </summary>
    public long LikeCount { get; set; }

    /// <summary>
    /// Gets or sets the title of the music track.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail image for the music track.
    /// </summary>
    public Image? Thumbnail { get; set; }
}
