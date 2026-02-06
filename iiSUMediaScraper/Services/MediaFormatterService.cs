using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using ImageMagick;
using ImageMagick.Drawing;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Provides media processing services including cropping, resizing, rounding corners, and applying overlays.
/// Uses ImageMagick for high-quality image manipulation.
/// </summary>
public class MediaFormatterService : IMediaFormatterService
{
    static MediaFormatterService()
    {
        // Enable multi-threading in ImageMagick for better CPU utilization
        ResourceLimits.Thread = (ulong)Environment.ProcessorCount;
    }

    /// <summary>
    /// Initializes a new instance of the MediaFormatterService.
    /// </summary>
    /// <param name="configurationService">Service for accessing image dimensions and overlay configurations.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public MediaFormatterService(IConfigurationService configurationService, IFileService fileService, ILogger<MediaFormatterService> logger)
    {
        ConfigurationService = configurationService;
        FileService = fileService;
        Logger = logger;
    }

    #region Core Image Operations

    /// <summary>
    /// Loads image bytes from LocalPath.
    /// </summary>
    /// <param name="image">The image to load bytes from.</param>
    /// <returns>The image bytes, or empty array if LocalPath is not set or file doesn't exist.</returns>
    private async Task<byte[]> LoadImageBytes(Image image)
    {
        if (string.IsNullOrWhiteSpace(image.LocalPath) || !File.Exists(image.LocalPath))
        {
            return [];
        }

        return await File.ReadAllBytesAsync(image.LocalPath);
    }

    /// <summary>
    /// Writes image bytes to a new temporary file and returns the path.
    /// </summary>
    /// <param name="bytes">The image bytes to write.</param>
    /// <returns>The path to the temporary file.</returns>
    private async Task<string> WriteToTemporaryFile(byte[] bytes)
    {
        var tempPath = await FileService.CreateTemporaryFile();
        await File.WriteAllBytesAsync(tempPath, bytes);
        return tempPath;
    }

    /// <summary>
    /// Applies fast PNG compression for local temporary files.
    /// Uses level 1 for speed since files are temporary and not stored permanently.
    /// </summary>
    /// <param name="image">The image to optimize.</param>
    private static void OptimizePngCompression(MagickImage image)
    {
        // Fast PNG compression - level 1 is much faster than level 9
        // Files are temporary so size is less important than speed
        image.Settings.SetDefine(MagickFormat.Png, "compression-level", "1");
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

        var result = new MagickImage(MagickColors.Transparent, overlay.Width, overlay.Height);
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
    /// <param name="cancellationToken">Token to cancel the operation. Returns null if cancelled.</param>
    /// <returns>The calculated crop region, or null if dimensions are not specified or operation was cancelled.</returns>
    private async Task<Crop?> SmartCrop(Image image, int? targetWidth, int? targetHeight, CancellationToken cancellationToken)
    {
        if (targetWidth == null || targetHeight == null)
        {
            return null;
        }

        var bytes = await LoadImageBytes(image);

        if (bytes.Length == 0)
        {
            return null;
        }

        return await Task.Run(() =>
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                Smartcrop.Result? crop = null;

                // First, try SmartCrop with the raw bytes - this works for most images
                try
                {
                    crop = new Smartcrop.ImageCrop((int)targetWidth, (int)targetHeight).Crop(bytes);
                }
                catch
                {
                    // Fallback: Convert image to RGBA format as Smartcrop only supports Bgra8888 and Rgba8888
                    byte[] processedBytes;
                    using (var magickImage = new MagickImage(bytes))
                    {
                        // Force conversion to TrueColorAlpha (RGBA) for SmartCrop compatibility
                        magickImage.ColorSpace = ColorSpace.sRGB;
                        magickImage.Alpha(AlphaOption.Set);

                        // Create a transparent RGBA canvas and composite the image onto it
                        using var canvas = new MagickImage(MagickColors.Transparent, magickImage.Width, magickImage.Height);
                        canvas.ColorSpace = ColorSpace.sRGB;
                        canvas.ColorType = ColorType.TrueColorAlpha;
                        canvas.Depth = 8;
                        canvas.Composite(magickImage, CompositeOperator.Over);

                        // Use Png32 format to force 32-bit RGBA output
                        processedBytes = canvas.ToByteArray(MagickFormat.Png32);
                    }

                    // Retry SmartCrop with converted bytes
                    crop = new Smartcrop.ImageCrop((int)targetWidth, (int)targetHeight).Crop(processedBytes);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

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
        }, cancellationToken);
    }

    /// <summary>
    /// Calculates smart crop for a hero image using configured dimensions.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns null if cancelled.</param>
    /// <returns>The calculated crop region, or null if cancelled or if image is from Previous source.</returns>
    public Task<Crop?> SmartCropHero(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Crop?>(null);

        var config = ConfigurationService.Configuration;

        return SmartCrop(image, config?.HeroWidth, config?.HeroHeight, cancellationToken);
    }

    /// <summary>
    /// Calculates smart crop for an icon image using configured dimensions.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns null if cancelled.</param>
    /// <returns>The calculated crop region, or null if cancelled or if image is from Previous source.</returns>
    public Task<Crop?> SmartCropIcon(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Crop?>(null);

        var config = ConfigurationService.Configuration;

        return SmartCrop(image, config?.IconWidth, config?.IconHeight, cancellationToken);
    }

    /// <summary>
    /// Calculates smart crop for a logo image using configured dimensions.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns null if cancelled.</param>
    /// <returns>The calculated crop region, or null if cancelled or if image is from Previous source.</returns>
    public Task<Crop?> SmartCropLogo(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Crop?>(null);

        var config = ConfigurationService.Configuration;

        return SmartCrop(image, config?.LogoWidth, config?.LogoHeight, cancellationToken);
    }

    /// <summary>
    /// Calculates smart crop for a slide image using configured dimensions.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns null if cancelled.</param>
    /// <returns>The calculated crop region, or null if cancelled or if image is from Previous source.</returns>
    public Task<Crop?> SmartCropSlide(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Crop?>(null);

        var config = ConfigurationService.Configuration;

        return SmartCrop(image, config?.SlideWidth, config?.SlideHeight, cancellationToken);
    }

    /// <summary>
    /// Calculates smart crop for a title image using configured dimensions.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns null if cancelled.</param>
    /// <returns>The calculated crop region, or null if cancelled or if image is from Previous source.</returns>
    public Task<Crop?> SmartCropTitle(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Crop?>(null);

        var config = ConfigurationService.Configuration;

        return SmartCrop(image, config?.TitleWidth, config?.TitleHeight, cancellationToken);
    }

    #endregion

    #region Format Image

    /// <summary>
    /// Formats an image by cropping, resizing, and optionally applying post-processing.
    /// Loads from LocalPath, processes, and writes result to a new temporary file.
    /// </summary>
    /// <param name="image">The image to format.</param>
    /// <param name="targetWidth">Target width after processing.</param>
    /// <param name="targetHeight">Target height after processing.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns the original image if cancelled.</param>
    /// <param name="applyRoundCorners">Whether to round the corners.</param>
    /// <param name="cornerRadius">Corner radius in pixels if rounding is applied.</param>
    /// <param name="postProcess">Optional custom post-processing function.</param>
    /// <returns>The formatted image with updated LocalPath, or the original image if cancelled or processing cannot be performed.</returns>
    private async Task<Image?> FormatImage(
        Image image,
        int? targetWidth,
        int? targetHeight,
        CancellationToken cancellationToken,
        bool applyRoundCorners = false,
        int cornerRadius = 80,
        Func<MagickImage, byte[]>? postProcess = null)
    {
        try
        {
            if (ConfigurationService.Configuration == null)
            {
                return image;
            }

            if (targetWidth == null && targetHeight == null)
            {
                return image;
            }

            if (image.Crop == null)
            {
                return image;
            }

            var bytes = await LoadImageBytes(image);
            if (bytes.Length == 0)
            {
                return image;
            }

            // Check if this is an animated image (WebP or GIF)
            var extension = image.Extension?.ToLower().Replace(".", "") ?? "";
            var isAnimatedFormat = extension == "webp" || extension == "gif";

            if (isAnimatedFormat)
            {
                return await FormatAnimatedImage(image, bytes, targetWidth, targetHeight, cancellationToken);
            }

            return await Task.Run(async () =>
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        return image;

                    var newImage = new Image { Url = image.Url, Extension = ".png" };

                    using MagickImage magickImage = new MagickImage(bytes);

                    magickImage.Format = MagickFormat.Png;
                    OptimizePngCompression(magickImage);

                    if (cancellationToken.IsCancellationRequested)
                        return image;

                    // Apply crop if present
                    if (image.Crop != null)
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

                    if (cancellationToken.IsCancellationRequested)
                        return image;

                    // Resize
                    magickImage.Resize(
                        (uint)(targetWidth ?? 0),
                        (uint)(targetHeight ?? 0),
                        FilterType.LanczosSharp);

                    if (cancellationToken.IsCancellationRequested)
                        return image;

                    // Apply post-processing or default behavior
                    byte[] resultBytes;
                    if (postProcess != null)
                    {
                        resultBytes = postProcess(magickImage);
                    }
                    else
                    {
                        if (applyRoundCorners)
                        {
                            RoundCorners(magickImage, cornerRadius);
                        }
                        resultBytes = magickImage.ToByteArray();
                    }

                    // Write to new temporary file and update LocalPath
                    newImage.LocalPath = await WriteToTemporaryFile(resultBytes);

                    return (Image?)newImage;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to format image to {Width}x{Height}", targetWidth, targetHeight);
                    return image;
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return image;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to format image to {Width}x{Height}", targetWidth, targetHeight);
            return image;
        }
    }

    /// <summary>
    /// Formats an animated image (WebP or GIF) while preserving animation.
    /// Applies crop and scale to all frames.
    /// </summary>
    private async Task<Image?> FormatAnimatedImage(
        Image image,
        byte[] bytes,
        int? targetWidth,
        int? targetHeight,
        CancellationToken cancellationToken)
    {
        return await Task.Run(async () =>
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return image;

                using var collection = new MagickImageCollection(bytes);

                // If only 1 frame, it's not actually animated - fall back to static processing
                if (collection.Count <= 1)
                {
                    Logger.LogDebug("Image has only {Count} frame(s), treating as static image", collection.Count);

                    // Process as static image
                    var newImage = new Image { Url = image.Url, Extension = ".png" };

                    using MagickImage magickImage = new MagickImage(bytes);
                    magickImage.Format = MagickFormat.Png;
                    OptimizePngCompression(magickImage);

                    if (image.Crop != null)
                    {
                        ApplyCrop(magickImage, image.Crop);
                        newImage.Width = image.Crop.Width;
                        newImage.Height = image.Crop.Height;
                    }

                    magickImage.Resize(
                        (uint)(targetWidth ?? 0),
                        (uint)(targetHeight ?? 0),
                        FilterType.LanczosSharp);

                    newImage.LocalPath = await WriteToTemporaryFile(magickImage.ToByteArray());
                    return (Image?)newImage;
                }

                Logger.LogDebug("Processing animated image with {Count} frames", collection.Count);

                // Coalesce to handle optimized GIFs/WebPs (fills in transparency from previous frames)
                collection.Coalesce();

                if (cancellationToken.IsCancellationRequested)
                    return image;

                // Process each frame in-place (sequential but memory efficient)
                foreach (var frame in collection)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return image;

                    // Apply crop if present
                    if (image.Crop != null)
                    {
                        frame.Crop(new MagickGeometry(
                            image.Crop.Left,
                            image.Crop.Top,
                            (uint)image.Crop.Width,
                            (uint)image.Crop.Height));
                        // Reset page geometry after crop
                        frame.Page = new MagickGeometry(0, 0, frame.Width, frame.Height);
                    }

                    // Use fast Box filter for resize (much faster than default Lanczos)
                    frame.FilterType = FilterType.Box;
                    frame.Resize(
                        (uint)(targetWidth ?? 0),
                        (uint)(targetHeight ?? 0));
                }

                if (cancellationToken.IsCancellationRequested)
                    return image;

                // Skip optimization for speed - file size doesn't matter for temporary files

                // Set format to animated WebP with fast encoding settings
                foreach (var frame in collection)
                {
                    frame.Format = MagickFormat.WebP;
                    // Use method 0 (fastest encoding) - affects speed, not quality
                    frame.Settings.SetDefine(MagickFormat.WebP, "method", "0");
                }

                var resultBytes = collection.ToByteArray(MagickFormat.WebP);

                var animatedImage = new Image
                {
                    Url = image.Url,
                    Extension = ".webp",
                    Width = targetWidth ?? image.Crop?.Width ?? 0,
                    Height = targetHeight ?? image.Crop?.Height ?? 0
                };

                animatedImage.LocalPath = await WriteToTemporaryFile(resultBytes);

                Logger.LogDebug("Formatted animated image: {Width}x{Height} with {Count} frames",
                    animatedImage.Width, animatedImage.Height, collection.Count);

                return (Image?)animatedImage;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to format animated image to {Width}x{Height}", targetWidth, targetHeight);
                return image;
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Updates an Image object's Width and Height properties based on its LocalPath file.
    /// </summary>
    /// <param name="image">The image to update.</param>
    public async Task UpdateImageSize(Image image)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(image.LocalPath) || !File.Exists(image.LocalPath))
            {
                return;
            }

            var info = new MagickImageInfo(image.LocalPath);

            image.Width = (int)info.Width;
            image.Height = (int)info.Height;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update image size");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Formats a hero image to configured dimensions.
    /// </summary>
    /// <param name="image">The hero image to format.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns the original image if cancelled.</param>
    /// <returns>The formatted image, the original image if cancelled, or unchanged if from Previous source.</returns>
    public Task<Image?> FormatHero(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Image?>(image);

        var config = ConfigurationService.Configuration;

        return FormatImage(image, config?.HeroWidth, config?.HeroHeight, cancellationToken);
    }

    /// <summary>
    /// Formats an icon image to configured dimensions and optionally applies platform overlay.
    /// </summary>
    /// <param name="image">The icon image to format.</param>
    /// <param name="platform">The gaming platform to determine which overlay to apply.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns the original image if cancelled.</param>
    /// <returns>The formatted image, the original image if cancelled, or unchanged if from Previous source.</returns>
    public Task<Image?> FormatIcon(Image image, string platform, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Image?>(image);

        var config = ConfigurationService.Configuration;

        return FormatImage(image, config?.IconWidth, config?.IconHeight, cancellationToken, postProcess: magickImage =>
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
    /// <param name="image">The logo image to format.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns the original image if cancelled.</param>
    /// <returns>The formatted image, the original image if cancelled, or unchanged if from Previous source.</returns>
    public Task<Image?> FormatLogo(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Image?>(image);

        var config = ConfigurationService.Configuration;

        return FormatImage(image, config?.LogoWidth, config?.LogoHeight, cancellationToken);
    }

    /// <summary>
    /// Formats a slide image to configured dimensions.
    /// </summary>
    /// <param name="image">The slide image to format.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns the original image if cancelled.</param>
    /// <returns>The formatted image, the original image if cancelled, or unchanged if from Previous source.</returns>
    public Task<Image?> FormatSlide(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Image?>(image);

        var config = ConfigurationService.Configuration;

        return FormatImage(image, config?.SlideWidth, config?.SlideHeight, cancellationToken);
    }

    /// <summary>
    /// Formats a title image to configured dimensions with rounded corners.
    /// </summary>
    /// <param name="image">The title image to format.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Returns the original image if cancelled.</param>
    /// <returns>The formatted image, the original image if cancelled, or unchanged if from Previous source.</returns>
    public Task<Image?> FormatTitle(Image image, CancellationToken cancellationToken = default)
    {
        if (image.Source == SourceFlag.Previous)
            return Task.FromResult<Image?>(image);

        var config = ConfigurationService.Configuration;

        return FormatImage(image, config?.TitleWidth, config?.TitleHeight, cancellationToken, applyRoundCorners: true);
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

        var newCrop = new Crop
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

    #region Format Video

    /// <summary>
    /// Gets the target dimensions for a video based on its ApplyMediaType.
    /// </summary>
    /// <param name="mediaType">The media type to get dimensions for.</param>
    /// <returns>A tuple of (width, height), or (null, null) if no dimensions configured.</returns>
    private (int? width, int? height) GetVideoDimensions(MediaType mediaType)
    {
        var config = ConfigurationService.Configuration;
        if (config == null)
            return (null, null);

        return mediaType switch
        {
            MediaType.Icon => (config.IconWidth, config.IconHeight),
            MediaType.Logo => (config.LogoWidth, config.LogoHeight),
            MediaType.Title => (config.TitleWidth, config.TitleHeight),
            MediaType.Hero => (config.HeroWidth, config.HeroHeight),
            MediaType.Slide => (config.SlideWidth, config.SlideHeight),
            _ => (null, null)
        };
    }

    /// <summary>
    /// Gets the duration of a video in seconds using ffprobe.
    /// </summary>
    /// <param name="videoPath">Path to the video file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Video duration in seconds, or null if duration cannot be determined.</returns>
    private async Task<double?> GetVideoDuration(string videoPath, CancellationToken cancellationToken)
    {
        try
        {
            var ffprobePath = Path.Combine(ConfigurationService.ToolsFolder, "ffprobe.exe");

            // If ffprobe doesn't exist, try using ffmpeg with format detection
            if (!File.Exists(ffprobePath))
            {
                ffprobePath = Path.Combine(ConfigurationService.ToolsFolder, "ffmpeg.exe");
                if (!File.Exists(ffprobePath))
                {
                    Logger.LogWarning("Neither ffprobe nor ffmpeg found in tools folder");
                    return null;
                }

                // Use ffmpeg -i to get duration from stderr output
                var ffmpegStartInfo = new ProcessStartInfo
                {
                    FileName = ffprobePath,
                    Arguments = $"-i \"{videoPath}\" -f null -",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var ffmpegProcess = new Process { StartInfo = ffmpegStartInfo };
                ffmpegProcess.Start();

                var stderrTask = ffmpegProcess.StandardError.ReadToEndAsync(cancellationToken);
                await ffmpegProcess.WaitForExitAsync(cancellationToken);
                var stderr = await stderrTask;

                // Parse duration from ffmpeg output: "Duration: 00:01:23.45"
                var durationMatch = System.Text.RegularExpressions.Regex.Match(stderr, @"Duration:\s*(\d+):(\d+):(\d+\.?\d*)");
                if (durationMatch.Success)
                {
                    var hours = double.Parse(durationMatch.Groups[1].Value);
                    var minutes = double.Parse(durationMatch.Groups[2].Value);
                    var seconds = double.Parse(durationMatch.Groups[3].Value);
                    return hours * 3600 + minutes * 60 + seconds;
                }

                return null;
            }

            // Use ffprobe for more reliable duration detection
            var arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var output = await outputTask;

            if (process.ExitCode == 0 && double.TryParse(output.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var duration))
            {
                Logger.LogDebug("Video duration: {Duration}s for {Path}", duration, videoPath);
                return duration;
            }

            return null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to get video duration for {Path}", videoPath);
            return null;
        }
    }

    /// <summary>
    /// Extracts a frame from a video at a specific timestamp.
    /// </summary>
    /// <param name="videoPath">Path to the video file.</param>
    /// <param name="timestampSeconds">Timestamp in seconds to extract the frame from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An Image object with the extracted frame, or null if extraction failed.</returns>
    private async Task<Image?> ExtractFrameAtTime(string videoPath, double timestampSeconds, CancellationToken cancellationToken)
    {
        try
        {
            var outputPath = await FileService.CreateTemporaryFile();
            var outputFile = Path.ChangeExtension(outputPath, ".png");

            // Format timestamp as HH:MM:SS.mmm
            var timeSpan = TimeSpan.FromSeconds(timestampSeconds);
            var timestamp = timeSpan.ToString(@"hh\:mm\:ss\.fff");

            // Extract frame at specific timestamp as PNG, using rgb24 to strip alpha channel
            var arguments = $"-ss {timestamp} -i \"{videoPath}\" -vframes 1 -pix_fmt rgb24 -y \"{outputFile}\"";
            var ffmpegPath = Path.Combine(ConfigurationService.ToolsFolder, "ffmpeg.exe");

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            await stderrTask;

            if (process.ExitCode != 0 || !File.Exists(outputFile))
            {
                Logger.LogDebug("ffmpeg failed to extract frame at {Timestamp}s", timestampSeconds);
                return null;
            }

            var image = new Image
            {
                LocalPath = outputFile,
                Extension = ".png"
            };

            await UpdateImageSize(image);

            Logger.LogDebug("Extracted frame at {Timestamp}s: {Width}x{Height}", timestampSeconds, image.Width, image.Height);

            return image;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to extract frame at {Timestamp}s from video", timestampSeconds);
            return null;
        }
    }

    /// <summary>
    /// Extracts the first frame from a video as a PNG image.
    /// </summary>
    /// <param name="videoPath">Path to the video file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An Image object with the first frame, or null if extraction failed.</returns>
    private async Task<Image?> ExtractFirstFrame(string videoPath, CancellationToken cancellationToken)
    {
        try
        {
            var outputPath = await FileService.CreateTemporaryFile();
            var outputFile = Path.ChangeExtension(outputPath, ".png");

            // Extract first frame as PNG, using rgb24 to strip alpha channel
            // Some videos (like webm) have alpha channels that can cause issues
            var arguments = $"-i \"{videoPath}\" -vframes 1 -pix_fmt rgb24 -y \"{outputFile}\"";
            var ffmpegPath = Path.Combine(ConfigurationService.ToolsFolder, "ffmpeg.exe");

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Read stderr asynchronously to prevent deadlocks
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var stderr = await stderrTask;

            if (process.ExitCode != 0 || !File.Exists(outputFile))
            {
                Logger.LogDebug("ffmpeg failed to extract frame (exit code {ExitCode}), trying ImageMagick fallback", process.ExitCode);

                // Fallback to ImageMagick for formats ffmpeg can't handle (like animated WebP)
                return await ExtractFirstFrameWithImageMagick(videoPath, outputFile, cancellationToken);
            }

            var image = new Image
            {
                LocalPath = outputFile,
                Extension = ".png"
            };

            // Get dimensions
            await UpdateImageSize(image);

            return image;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to extract first frame from video");
            return null;
        }
    }

    /// <summary>
    /// Extracts the first frame using ImageMagick as a fallback for formats ffmpeg can't handle.
    /// </summary>
    private async Task<Image?> ExtractFirstFrameWithImageMagick(string videoPath, string outputFile, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                // Read the first frame (index 0) from animated formats
                using var collection = new MagickImageCollection(videoPath);
                if (collection.Count == 0)
                {
                    Logger.LogWarning("ImageMagick could not read any frames from {Path}", videoPath);
                    return null;
                }

                using var firstFrame = collection[0];

                // Convert to RGB to strip any alpha channel
                firstFrame.ColorType = ColorType.TrueColor;
                firstFrame.Format = MagickFormat.Png;

                firstFrame.Write(outputFile);

                if (!File.Exists(outputFile))
                {
                    return null;
                }

                var image = new Image
                {
                    LocalPath = outputFile,
                    Extension = ".png",
                    Width = (int)firstFrame.Width,
                    Height = (int)firstFrame.Height
                };

                Logger.LogDebug("Extracted first frame using ImageMagick: {Width}x{Height}", image.Width, image.Height);

                return image;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "ImageMagick fallback also failed to extract first frame");
            return null;
        }
    }

    /// <summary>
    /// Calculates smart crop for a video by analyzing multiple frames sampled every 10 seconds.
    /// Computes an averaged crop region across all sampled frames for more consistent results.
    /// </summary>
    /// <param name="video">The video to analyze.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The calculated crop region, or null if smart crop cannot be determined or if video is from Previous source.</returns>
    public async Task<Crop?> SmartCropVideo(Video video, CancellationToken cancellationToken = default)
    {
        if (video.Source == SourceFlag.Previous)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(video.LocalPath) || !File.Exists(video.LocalPath))
        {
            return null;
        }

        var (targetWidth, targetHeight) = GetVideoDimensions(video.ApplyMediaType);
        if (targetWidth == null || targetHeight == null)
        {
            return null;
        }

        // Get video duration to determine sampling points
        var duration = await GetVideoDuration(video.LocalPath, cancellationToken);

        // If we can't get duration, fall back to first frame only
        if (duration == null || duration <= 0)
        {
            Logger.LogDebug("Could not determine video duration, falling back to first frame analysis");
            return await SmartCropVideoSingleFrame(video.LocalPath, targetWidth.Value, targetHeight.Value, cancellationToken);
        }

        // Calculate sample timestamps every 10 seconds, starting from 0
        const double sampleInterval = 10.0;
        var sampleTimestamps = new List<double> { 0 }; // Always include first frame

        for (double t = sampleInterval; t < duration.Value; t += sampleInterval)
        {
            sampleTimestamps.Add(t);
        }

        Logger.LogDebug("Sampling {Count} frames from video (duration: {Duration}s)", sampleTimestamps.Count, duration.Value);

        // Extract frames and compute crops
        var crops = new List<Crop>();
        var extractedFrames = new List<Image>();

        try
        {
            foreach (var timestamp in sampleTimestamps)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var frame = timestamp == 0
                    ? await ExtractFirstFrame(video.LocalPath, cancellationToken)
                    : await ExtractFrameAtTime(video.LocalPath, timestamp, cancellationToken);

                if (frame != null)
                {
                    extractedFrames.Add(frame);

                    var crop = await SmartCrop(frame, targetWidth, targetHeight, cancellationToken);
                    if (crop != null)
                    {
                        crops.Add(crop);
                        Logger.LogDebug("Frame at {Timestamp}s: crop ({Left}, {Top}) {Width}x{Height}",
                            timestamp, crop.Left, crop.Top, crop.Width, crop.Height);
                    }
                }
            }

            if (crops.Count == 0)
            {
                Logger.LogWarning("No valid crops computed from sampled frames");
                return null;
            }

            // Compute averaged crop region
            // Use the median for position (more robust to outliers) and the mode/average for size
            var sortedLefts = crops.Select(c => c.Left).OrderBy(x => x).ToList();
            var sortedTops = crops.Select(c => c.Top).OrderBy(x => x).ToList();
            var sortedWidths = crops.Select(c => c.Width).OrderBy(x => x).ToList();
            var sortedHeights = crops.Select(c => c.Height).OrderBy(x => x).ToList();

            // Use median values for more stable results
            var medianIndex = crops.Count / 2;

            var averagedCrop = new Crop
            {
                Left = sortedLefts[medianIndex],
                Top = sortedTops[medianIndex],
                Width = sortedWidths[medianIndex],
                Height = sortedHeights[medianIndex]
            };

            Logger.LogDebug("Averaged crop from {Count} frames: ({Left}, {Top}) {Width}x{Height}",
                crops.Count, averagedCrop.Left, averagedCrop.Top, averagedCrop.Width, averagedCrop.Height);

            return averagedCrop;
        }
        finally
        {
            // Clean up all extracted frame files
            foreach (var frame in extractedFrames)
            {
                if (!string.IsNullOrWhiteSpace(frame.LocalPath) && File.Exists(frame.LocalPath))
                {
                    try { File.Delete(frame.LocalPath); } catch { }
                }
            }
        }
    }

    /// <summary>
    /// Fallback method to compute smart crop from a single frame when duration is unavailable.
    /// </summary>
    private async Task<Crop?> SmartCropVideoSingleFrame(string videoPath, int targetWidth, int targetHeight, CancellationToken cancellationToken)
    {
        var firstFrame = await ExtractFirstFrame(videoPath, cancellationToken);
        if (firstFrame == null)
        {
            return null;
        }

        try
        {
            return await SmartCrop(firstFrame, targetWidth, targetHeight, cancellationToken);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(firstFrame.LocalPath) && File.Exists(firstFrame.LocalPath))
            {
                try { File.Delete(firstFrame.LocalPath); } catch { }
            }
        }
    }

    /// <summary>
    /// Formats a video by smart cropping (based on first frame analysis) and scaling to target dimensions.
    /// Uses ffmpeg to process the video.
    /// </summary>
    /// <param name="video">The video to format. Must have Crop set (use SmartCropVideo first).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The formatted video with updated LocalPath, the original video if processing failed, or unchanged if from Previous source.</returns>
    public async Task<Video?> FormatVideo(Video video, CancellationToken cancellationToken = default)
    {
        if (video.Source == SourceFlag.Previous)
        {
            return video;
        }

        if (string.IsNullOrWhiteSpace(video.LocalPath) || !File.Exists(video.LocalPath))
        {
            return video;
        }

        var (targetWidth, targetHeight) = GetVideoDimensions(video.ApplyMediaType);
        if (targetWidth == null || targetHeight == null)
        {
            return video;
        }

        try
        {
            var outputPath = await FileService.CreateTemporaryFile();
            var inputExtension = Path.GetExtension(video.LocalPath);
            var outputFile = Path.ChangeExtension(outputPath, inputExtension);

            var ffmpegPath = Path.Combine(ConfigurationService.ToolsFolder, "ffmpeg.exe");

            // Build video filter: crop (if available), then scale, then convert to yuv420p
            // The format=yuv420p is required because some videos (like webm) may have alpha channels
            // which libx264 doesn't support - this strips the alpha channel
            string videoFilter;
            if (video.Crop != null)
            {
                // Crop first, then scale to target dimensions, then strip alpha
                var cropFilter = $"crop={video.Crop.Width}:{video.Crop.Height}:{video.Crop.Left}:{video.Crop.Top}";
                var scaleFilter = $"scale={targetWidth}:{targetHeight}";
                videoFilter = $"{cropFilter},{scaleFilter},format=yuv420p";

                Logger.LogDebug("Formatting video with crop ({CropW}x{CropH} at {CropX},{CropY}) and scale to {Width}x{Height}",
                    video.Crop.Width, video.Crop.Height, video.Crop.Left, video.Crop.Top, targetWidth, targetHeight);
            }
            else
            {
                // No crop, just scale with aspect ratio preservation and padding, then strip alpha
                videoFilter = $"scale={targetWidth}:{targetHeight}:force_original_aspect_ratio=decrease,pad={targetWidth}:{targetHeight}:(ow-iw)/2:(oh-ih)/2,format=yuv420p";

                Logger.LogDebug("Formatting video with scale to {Width}x{Height} (no crop)", targetWidth, targetHeight);
            }

            // Determine audio handling based on media type
            // Hero videos are displayed without audio, Slide videos need audio
            bool stripAudio = video.ApplyMediaType == MediaType.Hero;
            string audioArgs = stripAudio ? "-an" : "-c:a aac -b:a 128k";

            Logger.LogDebug("{Action} audio for {MediaType}", stripAudio ? "Stripping" : "Including", video.ApplyMediaType);

            var arguments = $"-i \"{video.LocalPath}\" -vf \"{videoFilter}\" -c:v libx264 -preset fast -crf 23 {audioArgs} -y \"{outputFile}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Read stderr asynchronously to prevent deadlocks when buffer fills
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var stderr = await stderrTask;

            // If we tried to include audio but it failed, retry without audio
            // This handles cases where the input file has no audio stream
            if (process.ExitCode != 0 && !stripAudio)
            {
                Logger.LogDebug("ffmpeg failed with audio (exit code {ExitCode}), retrying without audio. Error: {Error}", process.ExitCode, stderr);

                // Retry with -an (no audio) - videoFilter already includes format=yuv420p
                var retryArguments = $"-i \"{video.LocalPath}\" -vf \"{videoFilter}\" -c:v libx264 -preset fast -crf 23 -an -y \"{outputFile}\"";

                var retryStartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = retryArguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var retryProcess = new Process { StartInfo = retryStartInfo };
                retryProcess.Start();

                // Read stderr asynchronously for retry
                var retryStderrTask = retryProcess.StandardError.ReadToEndAsync(cancellationToken);

                await retryProcess.WaitForExitAsync(cancellationToken);

                var retryStderr = await retryStderrTask;

                if (retryProcess.ExitCode != 0)
                {
                    Logger.LogWarning("ffmpeg retry without audio exited with code {ExitCode}: {Error}", retryProcess.ExitCode, retryStderr);
                    return video;
                }
            }
            else if (process.ExitCode != 0)
            {
                Logger.LogWarning("ffmpeg exited with code {ExitCode}: {Error}", process.ExitCode, stderr);
                return video;
            }

            if (File.Exists(outputFile))
            {
                var newVideo = new Video
                {
                    Url = video.Url,
                    LocalPath = outputFile,
                    Extension = inputExtension,
                    ApplyMediaType = video.ApplyMediaType,
                    Duration = video.Duration,
                    Title = video.Title,
                    Thumbnail = video.Thumbnail,
                    Source = video.Source
                };

                Logger.LogDebug("Video formatted successfully to {Width}x{Height}", targetWidth, targetHeight);
                return newVideo;
            }

            return video;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to format video to {Width}x{Height}", targetWidth, targetHeight);
            return video;
        }
    }

    #endregion

    /// <summary>
    /// Gets the configuration service for accessing application settings.
    /// </summary>
    protected IConfigurationService ConfigurationService { get; private set; }

    /// <summary>
    /// Gets the file service for file operations.
    /// </summary>
    protected IFileService FileService { get; private set; }

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }
}
