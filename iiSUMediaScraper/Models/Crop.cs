namespace iiSUMediaScraper.Models;

/// <summary>
/// Represents a rectangular crop region for media items.
/// </summary>
public class Crop
{
    /// <summary>
    /// Gets or sets the left offset of the crop region in pixels.
    /// </summary>
    public int Left { get; set; }

    /// <summary>
    /// Gets or sets the top offset of the crop region in pixels.
    /// </summary>
    public int Top { get; set; }

    /// <summary>
    /// Gets or sets the width of the crop region in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the crop region in pixels.
    /// </summary>
    public int Height { get; set; }
}
