using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service for managing application configuration.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Saves the current configuration to storage.
    /// </summary>
    Task SaveConfiguration();

    /// <summary>
    /// Loads the configuration from storage.
    /// </summary>
    Task LoadConfiguration();

    /// <summary>
    /// Gets or sets the application configuration.
    /// </summary>
    Configuration? Configuration { get; set; }

    /// <summary>
    /// Gets the folder path where external tools (yt-dlp.exe, ffmpeg.exe, ffprobe.exe, ffplay.exe) are located.
    /// </summary>
    string ToolsFolder { get; }
}
