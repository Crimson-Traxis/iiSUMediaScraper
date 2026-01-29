namespace iiSUMediaScraper.Models.Upscale;

/// <summary>
/// Result of an upscale operation
/// </summary>
public class UpscaleResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public byte[]? ImageData { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}