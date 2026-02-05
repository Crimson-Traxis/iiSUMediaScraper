namespace iiSUMediaScraper.Models;

/// <summary>
/// Base class for all media types (images, videos, music).
/// </summary>
public class Media
{
    /// <summary>
    /// Gets or sets the remote URL of the media.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the file extension of the media.
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Gets or sets the source flag indicating where the media originated.
    /// </summary>
    public SourceFlag Source { get; set; }

    /// <summary>
    /// Gets or sets the local file path where the media is stored.
    /// </summary>
    public string? LocalPath { get; set; }

    /// <summary>
    /// Gets or sets the crop region for the media.
    /// </summary>
    public Crop? Crop { get; set; }
}
