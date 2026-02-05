namespace iiSUMediaScraper.Models;

/// <summary>
/// Represents an image media item with dimensions.
/// </summary>
public class Image : Media
{
    /// <summary>
    /// Gets or sets the width of the image in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the image in pixels.
    /// </summary>
    public int Height { get; set; }
}
