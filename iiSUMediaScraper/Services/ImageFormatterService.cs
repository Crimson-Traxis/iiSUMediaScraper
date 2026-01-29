using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using ImageMagick;
using ImageMagick.Drawing;
using Microsoft.Extensions.Logging;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Provides image processing services including cropping, resizing, rounding corners, and applying overlays.
/// Uses ImageMagick for high-quality image manipulation.
/// </summary>
public class ImageFormatterService : IImageFormatterService
{
    private readonly IConfigurationService _configurationService;

    /// <summary>
    /// Initializes a new instance of the ImageFormatterService.
    /// </summary>
    /// <param name="configurationService">Service for accessing image dimensions and overlay configurations.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public ImageFormatterService(IConfigurationService configurationService, ILogger<ImageFormatterService> logger)
    {
        _configurationService = configurationService;
        Logger = logger;
    }

    #region Core Image Operations

    /// <summary>
    /// Applies maximum lossless PNG compression to reduce file size.
    /// </summary>
    /// <param name="image">The image to optimize.</param>
    private static void OptimizePngCompression(MagickImage image)
    {
        // Lossless PNG compression optimization
        image.Settings.SetDefine(MagickFormat.Png, "compression-level", "9");
        image.Settings.SetDefine(MagickFormat.Png, "compression-filter", "5");
    }

    /// <summary>
    /// Rounds the corners of an image by applying an alpha mask.
    /// </summary>
    /// <param name="image">The image to modify.</param>
    /// <param name="cornerRadius">The radius of the rounded corners in pixels.</param>
    private static void RoundCorners(MagickImage image, int cornerRadius)
    {
        image.Alpha(AlphaOption.Set);

        using MagickImage mask = new MagickImage(MagickColors.Transparent, image.Width, image.Height);
        new Drawables()
            .FillColor(MagickColors.White)
            .RoundRectangle(0, 0, image.Width - 1, image.Height - 1, cornerRadius, cornerRadius)
            .Draw(mask);

        image.Composite(mask, CompositeOperator.DstIn);
        image.Format = MagickFormat.Png;
        OptimizePngCompression(image);
    }

    /// <summary>
    /// Crops an image to the specified region.
    /// </summary>
    /// <param name="magickImage">The image to crop.</param>
    /// <param name="crop">The crop region (left, top, width, height).</param>
    private static void ApplyCrop(MagickImage magickImage, Crop crop)
    {
        magickImage.Crop(new MagickGeometry()
        {
            X = crop.Left,
            Y = crop.Top,
            Width = (uint)crop.Width,
            Height = (uint)crop.Height,
        });
    }

    /// <summary>
    /// Adds a platform overlay/border image on top of a source image.
    /// The source image is resized to fit within the overlay's border, rounded, then composited.
    /// </summary>
    /// <param name="sourceImage">The base image to overlay.</param>
    /// <param name="overlayPath">Path to the overlay image file.</param>
    /// <param name="cornerRadius">Corner radius to apply to the source image.</param>
    /// <param name="borderThickness">Thickness of the overlay border in pixels.</param>
    /// <returns>A new image with the overlay applied.</returns>
    private static MagickImage AddOverlay(MagickImage sourceImage, string overlayPath, int cornerRadius, int borderThickness)
    {
        using MagickImage overlay = new MagickImage(overlayPath);

        int innerWidth = (int)overlay.Width - (borderThickness * 2);
        int innerHeight = (int)overlay.Height - (borderThickness * 2);

        MagickImage resizedImage = (MagickImage)sourceImage.Clone();
        resizedImage.Resize((uint)innerWidth, (uint)innerHeight);

        RoundCorners(resizedImage, cornerRadius);

        MagickImage result = new MagickImage(MagickColors.Transparent, overlay.Width, overlay.Height);
        result.Composite(resizedImage, borderThickness, borderThickness, CompositeOperator.Over);
        result.Composite(overlay, CompositeOperator.Over);
        result.Format = MagickFormat.Png;
        OptimizePngCompression(result);

        resizedImage.Dispose();
        return result;
    }

    #endregion

    #region Smart Crop

    /// <summary>
    /// Automatically determines the best crop region for an image using the SmartCrop library.
    /// Analyzes the image to find the most important area to preserve.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <param name="targetWidth">Target width for the cropped region.</param>
    /// <param name="targetHeight">Target height for the cropped region.</param>
    /// <returns>The calculated crop region, or null if dimensions are not specified.</returns>
    private Task<Crop?> SmartCrop(Image image, int? targetWidth, int? targetHeight)
    {
        if (targetWidth == null || targetHeight == null)
            return Task.FromResult<Crop?>(null);

        return Task.Run(() =>
        {
            try
            {
                var crop = new Smartcrop.ImageCrop((int)targetWidth, (int)targetHeight).Crop(image.Bytes);

                return new Crop()
                {
                    Left = crop.Area.Left,
                    Top = crop.Area.Top,
                    Width = crop.Area.Width,
                    Height = crop.Area.Height,
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to smart crop image to {Width}x{Height}", targetWidth, targetHeight);
                return null;
            }
        });
    }

    /// <summary>
    /// Calculates smart crop for a hero image using configured dimensions.
    /// </summary>
    public Task<Crop?> SmartCropHero(Image image)
    {
        var config = _configurationService.Configuration;
        return SmartCrop(image, config?.HeroWidth, config?.HeroHeight);
    }

    /// <summary>
    /// Calculates smart crop for an icon image using configured dimensions.
    /// </summary>
    public Task<Crop?> SmartCropIcon(Image image)
    {
        var config = _configurationService.Configuration;
        return SmartCrop(image, config?.IconWidth, config?.IconHeight);
    }

    /// <summary>
    /// Calculates smart crop for a logo image using configured dimensions.
    /// </summary>
    public Task<Crop?> SmartCropLogo(Image image)
    {
        var config = _configurationService.Configuration;
        return SmartCrop(image, config?.LogoWidth, config?.LogoHeight);
    }

    /// <summary>
    /// Calculates smart crop for a slide image using configured dimensions.
    /// </summary>
    public Task<Crop?> SmartCropSlide(Image image)
    {
        var config = _configurationService.Configuration;
        return SmartCrop(image, config?.SlideWidth, config?.SlideHeight);
    }

    /// <summary>
    /// Calculates smart crop for a title image using configured dimensions.
    /// </summary>
    public Task<Crop?> SmartCropTitle(Image image)
    {
        var config = _configurationService.Configuration;
        return SmartCrop(image, config?.TitleWidth, config?.TitleHeight);
    }

    #endregion

    #region Format Image

    /// <summary>
    /// Formats an image by cropping, resizing, and optionally applying post-processing.
    /// </summary>
    /// <param name="image">The image to format.</param>
    /// <param name="targetWidth">Target width after processing.</param>
    /// <param name="targetHeight">Target height after processing.</param>
    /// <param name="applyRoundCorners">Whether to round the corners.</param>
    /// <param name="cornerRadius">Corner radius in pixels if rounding is applied.</param>
    /// <param name="postProcess">Optional custom post-processing function.</param>
    /// <returns>The formatted image, or null if processing cannot be performed.</returns>
    private Task<Image?> FormatImage(
        Image image,
        int? targetWidth,
        int? targetHeight,
        bool applyRoundCorners = false,
        int cornerRadius = 80,
        Func<MagickImage, byte[]>? postProcess = null)
    {
        if (_configurationService.Configuration == null)
        {
            return Task.FromResult<Image?>(image);
        }

        if (targetWidth == null && targetHeight == null)
        {
            return Task.FromResult<Image?>(image);
        }

        if (image.Crop == null)
        {
            return Task.FromResult<Image?>(image);
        }

        return Task.Run(() =>
        {
            try
            {
                Image newImage = new Image { Url = image.Url, Extension = ".png" };

                using MagickImage magickImage = new MagickImage(image.Bytes);

                magickImage.Format = MagickFormat.Png;
                OptimizePngCompression(magickImage);

                // Apply crop if present
                if (image.Crop != null && image.Bytes.Length > 0)
                {
                    try
                    {
                        ApplyCrop(magickImage, image.Crop);
                        newImage.Width = image.Crop.Width;
                        newImage.Height = image.Crop.Height;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Failed to apply crop to image");
                    }
                }

                // Resize
                magickImage.Resize(
                    (uint)(targetWidth ?? 0),
                    (uint)(targetHeight ?? 0),
                    FilterType.LanczosSharp);

                // Apply post-processing or default behavior
                if (postProcess != null)
                {
                    newImage.Bytes = postProcess(magickImage);
                }
                else
                {
                    if (applyRoundCorners)
                    {
                        RoundCorners(magickImage, cornerRadius);
                    }
                    newImage.Bytes = magickImage.ToByteArray();
                }

                return (Image?)newImage;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to format image to {Width}x{Height}", targetWidth, targetHeight);
                return null;
            }
        });
    }

    /// <summary>
    /// Updates an Image object's Width and Height properties based on its byte data.
    /// </summary>
    /// <param name="image">The image to update.</param>
    public Task UpdateImageSize(Image image)
    {
        try
        {
            MagickImageInfo info = new MagickImageInfo(image.Bytes);

            image.Width = (int)info.Width;
            image.Height = (int)info.Height;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update image size");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Formats a hero image to configured dimensions.
    /// </summary>
    public Task<Image?> FormatHero(Image image)
    {
        var config = _configurationService.Configuration;
        return FormatImage(image, config?.HeroWidth, config?.HeroHeight);
    }

    /// <summary>
    /// Formats an icon image to configured dimensions and optionally applies platform overlay.
    /// </summary>
    /// <param name="image">The icon image to format.</param>
    /// <param name="platform">The gaming platform to determine which overlay to apply.</param>
    public Task<Image?> FormatIcon(Image image, string platform)
    {
        var config = _configurationService.Configuration;

        return FormatImage(image, config?.IconWidth, config?.IconHeight, postProcess: magickImage =>
        {
            var overlayConfig = config?.GameIconOverlayConfigurations
                .FirstOrDefault(c => c.Platform == platform);

            if (overlayConfig != null && !string.IsNullOrWhiteSpace(overlayConfig.Path))
            {
                using MagickImage overlayResult = AddOverlay(magickImage, overlayConfig.Path, 20, 20);
                return overlayResult.ToByteArray();
            }

            return magickImage.ToByteArray();
        });
    }

    /// <summary>
    /// Formats a logo image to configured dimensions.
    /// </summary>
    public Task<Image?> FormatLogo(Image image)
    {
        var config = _configurationService.Configuration;
        return FormatImage(image, config?.LogoWidth, config?.LogoHeight);
    }

    /// <summary>
    /// Formats a slide image to configured dimensions.
    /// </summary>
    public Task<Image?> FormatSlide(Image image)
    {
        var config = _configurationService.Configuration;
        return FormatImage(image, config?.SlideWidth, config?.SlideHeight);
    }

    /// <summary>
    /// Formats a title image to configured dimensions with rounded corners.
    /// </summary>
    public Task<Image?> FormatTitle(Image image)
    {
        var config = _configurationService.Configuration;
        return FormatImage(image, config?.TitleWidth, config?.TitleHeight, applyRoundCorners: true);
    }

    /// <summary>
    /// Calculates a new crop region when an image is resized.
    /// Scales the crop coordinates proportionally to the new image dimensions.
    /// </summary>
    /// <param name="oldWidth">Original image width.</param>
    /// <param name="oldHeight">Original image height.</param>
    /// <param name="oldCrop">Original crop region.</param>
    /// <param name="newWidth">New image width.</param>
    /// <param name="newHeight">New image height.</param>
    /// <returns>Scaled crop region clamped to new image bounds.</returns>
    public Task<Crop?> CalculateNewCrop(int oldWidth, int oldHeight, Crop oldCrop, int newWidth, int newHeight)
    {
        if (oldWidth <= 0 || oldHeight <= 0 || newWidth <= 0 || newHeight <= 0)
            return Task.FromResult<Crop?>(null);

        if (oldCrop == null)
            return Task.FromResult<Crop?>(null);

        double scaleX = (double)newWidth / oldWidth;
        double scaleY = (double)newHeight / oldHeight;

        Crop newCrop = new Crop
        {
            Left = (int)Math.Round(oldCrop.Left * scaleX),
            Top = (int)Math.Round(oldCrop.Top * scaleY),
            Width = (int)Math.Round(oldCrop.Width * scaleX),
            Height = (int)Math.Round(oldCrop.Height * scaleY)
        };

        // Clamp to image bounds
        newCrop.Left = Math.Clamp(newCrop.Left, 0, newWidth);
        newCrop.Top = Math.Clamp(newCrop.Top, 0, newHeight);
        newCrop.Width = Math.Clamp(newCrop.Width, 1, newWidth - newCrop.Left);
        newCrop.Height = Math.Clamp(newCrop.Height, 1, newHeight - newCrop.Top);

        return Task.FromResult<Crop?>(newCrop);
    }

    #endregion

    protected ILogger Logger { get; private set; }
}
