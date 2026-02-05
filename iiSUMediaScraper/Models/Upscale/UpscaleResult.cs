namespace iiSUMediaScraper.Models.Upscale;

/// <summary>
/// Result of an upscale operation.
/// </summary>
public class UpscaleResult
{
    /// <summary>
    /// Gets or sets whether the upscale operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the result message or error description.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the upscaled image data as bytes.
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// Gets or sets the width of the upscaled image.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the upscaled image.
    /// </summary>
    public int? Height { get; set; }
}