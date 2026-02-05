namespace iiSUMediaScraper.Models;

/// <summary>
/// Represents a video media item, extending music with video-specific properties.
/// </summary>
public class Video : Music
{
    /// <summary>
    /// Gets or sets the media type to apply when using this video.
    /// </summary>
    public MediaType ApplyMediaType { get; set; }
}
