using iiSUMediaScraper.Models;

namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service for formatting and processing media (images and videos).
/// </summary>
public interface IMediaFormatterService
{
    /// <summary>
    /// Updates the width and height properties of an image by reading the actual file.
    /// </summary>
    Task UpdateImageSize(Image image);

    /// <summary>
    /// Formats an image for use as an icon with optional platform overlay.
    /// </summary>
    Task<Image?> FormatIcon(Image image, string platform, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats an image for use as a title image.
    /// </summary>
    Task<Image?> FormatTitle(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats an image for use as a logo.
    /// </summary>
    Task<Image?> FormatLogo(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats an image for use as a hero image.
    /// </summary>
    Task<Image?> FormatHero(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats an image for use as a slide image.
    /// </summary>
    Task<Image?> FormatSlide(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates a smart crop for an icon image.
    /// </summary>
    Task<Crop?> SmartCropIcon(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates a smart crop for a title image.
    /// </summary>
    Task<Crop?> SmartCropTitle(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates a smart crop for a logo image.
    /// </summary>
    Task<Crop?> SmartCropLogo(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates a smart crop for a hero image.
    /// </summary>
    Task<Crop?> SmartCropHero(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates a smart crop for a slide image.
    /// </summary>
    Task<Crop?> SmartCropSlide(Image image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates crop coordinates for a new image resolution.
    /// </summary>
    Task<Crop?> CalculateNewCrop(int oldWidth, int oldHeight, Crop oldCrop, int newWidth, int newHeight);

    /// <summary>
    /// Calculates a smart crop for a video thumbnail.
    /// </summary>
    Task<Crop?> SmartCropVideo(Video video, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats a video for use based on its apply media type.
    /// </summary>
    Task<Video?> FormatVideo(Video video, CancellationToken cancellationToken = default);
}
