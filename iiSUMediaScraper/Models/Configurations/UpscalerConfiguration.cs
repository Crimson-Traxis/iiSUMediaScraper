namespace iiSUMediaScraper.Models.Configurations;

/// <summary>
/// Configuration for SeedVR2 image upscaling quality settings.
/// Resolution is passed separately per request.
/// </summary>
public class UpscalerConfiguration
{
    /// <summary>
    /// Job name for reference.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Maximum resolution for any edge. Automatically scales down if exceeded.
    /// 0 = no limit.
    /// Default: 0
    /// </summary>
    public int MaxResolution { get; set; } = 0;

    /// <summary>
    /// Random seed for reproducible generation.
    /// Default: 42
    /// </summary>
    public int Seed { get; set; } = 42;

    /// <summary>
    /// Color correction method.
    /// Options: "lab" (recommended), "wavelet", "wavelet_adaptive", "hsv", "adain", "none"
    /// Default: "lab"
    /// </summary>
    public string ColorCorrection { get; set; } = "lab";

    /// <summary>
    /// Input noise injection scale (0.0-1.0).
    /// Adds noise to input frames to reduce artifacts at very high resolutions.
    /// Try 0.02-0.1 for artifact reduction.
    /// Default: 0.0
    /// </summary>
    public double InputNoiseScale { get; set; } = 0.0;

    /// <summary>
    /// Latent space noise scale (0.0-1.0).
    /// Adds noise during diffusion process, can soften excessive detail.
    /// Default: 0.0
    /// </summary>
    public double LatentNoiseScale { get; set; } = 0.0;
}