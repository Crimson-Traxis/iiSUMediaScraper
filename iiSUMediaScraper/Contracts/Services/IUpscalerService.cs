using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Models.Upscale;

namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service interface for image upscaling operations.
/// </summary>
public interface IUpscalerService : IDisposable
{
    /// <summary>
    /// Checks if the upscaler server is healthy and the model is loaded.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the server is healthy and ready to process requests</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Upscales an image using the provided configuration.
    /// This method is thread-safe and ensures only one request is processed at a time.
    /// </summary>
    /// <param name="configuration">Upscaler configuration (quality settings)</param>
    /// <param name="imageData">Raw image bytes to upscale</param>
    /// <param name="targetWidth">Target output width in pixels</param>
    /// <param name="targetHeight">Target output height in pixels</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the upscaled image data</returns>
    Task<UpscaleResult> UpscaleAsync(
        UpscalerConfiguration configuration,
        byte[] imageData,
        int targetWidth,
        int targetHeight,
        CancellationToken cancellationToken = default);
}