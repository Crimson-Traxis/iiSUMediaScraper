namespace iiSUMediaScraper.Models.Configurations;

/// <summary>
/// Main application configuration containing all settings for media scraping and processing.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Gets or sets the root path where games are located.
    /// </summary>
    public string? GamesPath { get; set; }

    /// <summary>
    /// Gets or sets the path where applied assets will be saved.
    /// </summary>
    public string? ApplyAssetPath { get; set; }

    /// <summary>
    /// Gets or sets the path to move games with unfound media.
    /// </summary>
    public string? UnfoundMediaMovePath { get; set; }

    /// <summary>
    /// Gets or sets the SteamGridDB API key.
    /// </summary>
    public string? SteamGridDbKey { get; set; }

    /// <summary>
    /// Gets or sets the IGDB client secret.
    /// </summary>
    public string? IgdbClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the IGDB client ID.
    /// </summary>
    public string? IgdbClientId { get; set; }

    /// <summary>
    /// Gets or sets whether to prioritize logo images as title images.
    /// </summary>
    public bool IsPrioritizeLogoAsTitle { get; set; }

    /// <summary>
    /// Gets or sets whether to use logo images as title images.
    /// </summary>
    public bool IsUseLogoAsTitle { get; set; }

    /// <summary>
    /// Gets or sets whether to move unfound games to a separate folder.
    /// </summary>
    public bool IsMoveToUnfoundGamesFolder { get; set; }

    /// <summary>
    /// Gets or sets whether to apply icon images.
    /// </summary>
    public bool IsApplyIcon { get; set; }

    /// <summary>
    /// Gets or sets whether to apply title images.
    /// </summary>
    public bool IsApplyTitle { get; set; }

    /// <summary>
    /// Gets or sets whether to apply hero images.
    /// </summary>
    public bool IsApplyHeros { get; set; }

    /// <summary>
    /// Gets or sets whether to apply slide images.
    /// </summary>
    public bool IsApplySlides { get; set; }

    /// <summary>
    /// Gets or sets whether to apply music.
    /// </summary>
    public bool IsApplyMusic { get; set; }

    /// <summary>
    /// Gets or sets whether the previewer is enabled.
    /// </summary>
    public bool IsPreviewerEnabled { get; set; }

    /// <summary>
    /// Gets or sets the file name format for icon images.
    /// </summary>
    public string? IconNameFormat { get; set; }

    /// <summary>
    /// Gets or sets the file name format for title images.
    /// </summary>
    public string? TitleNameFormat { get; set; }

    /// <summary>
    /// Gets or sets the file name format for hero images.
    /// </summary>
    public string? HeroNameFormat { get; set; }

    /// <summary>
    /// Gets or sets the file name format for slide images.
    /// </summary>
    public string? SlideNameFormat { get; set; }

    /// <summary>
    /// Gets or sets the file name format for music.
    /// </summary>
    public string? MusicNameFormat { get; set; }

    /// <summary>
    /// Gets or sets whether to apply assets for unfound games.
    /// </summary>
    public bool IsApplyUnfoundGames { get; set; }

    /// <summary>
    /// Gets or sets whether to delete existing hero assets before applying new ones.
    /// </summary>
    public bool IsDeleteExistingHeroAssets { get; set; }

    /// <summary>
    /// Gets or sets whether to delete existing slide assets before applying new ones.
    /// </summary>
    public bool IsDeleteExistingSlideAssets { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of games to scrape concurrently.
    /// </summary>
    public int? MaxNumberOfConcurrentGames { get; set; }

    /// <summary>
    /// Gets or sets whether to load previously saved assets when scraping.
    /// </summary>
    public bool IsLoadPrevious { get; set; }

    /// <summary>
    /// Gets or sets the name of the default upscale configuration.
    /// </summary>
    public string? DefaultUpscaleConfigurationName { get; set; }

    /// <summary>
    /// Gets or sets the name of the default reconstructor configuration.
    /// </summary>
    public string? DefaultReconstructorConfigurationName { get; set; }

    /// <summary>
    /// Gets or sets whether to scan for games.
    /// </summary>
    public bool IsScanGames { get; set; }

    /// <summary>
    /// Gets or sets whether to scan for unfound games.
    /// </summary>
    public bool IsScanUnfoundGames { get; set; }

    /// <summary>
    /// Gets or sets whether to mark as unfound if no icons are available.
    /// </summary>
    public bool IsUnfoundMediaIfNoIcons { get; set; }

    /// <summary>
    /// Gets or sets whether to mark as unfound if no logos are available.
    /// </summary>
    public bool IsUnfoundMediaIfNoLogos { get; set; }

    /// <summary>
    /// Gets or sets whether to mark as unfound if no titles are available.
    /// </summary>
    public bool IsUnfoundMediaIfNoTitles { get; set; }

    /// <summary>
    /// Gets or sets whether to mark as unfound if no heroes are available.
    /// </summary>
    public bool IsUnfoundMediaIfNoHeros { get; set; }

    /// <summary>
    /// Gets or sets whether to mark as unfound if no slides are available.
    /// </summary>
    public bool IsUnfoundMediaIfNoSlides { get; set; }

    /// <summary>
    /// Gets or sets whether to mark as unfound if no music is available.
    /// </summary>
    public bool IsUnfoundMediaIfNoMusic { get; set; }

    /// <summary>
    /// Gets or sets whether to mark as unfound if no videos are available.
    /// </summary>
    public bool IsUnfoundMediaIfNoVideos { get; set; }

    /// <summary>
    /// Gets or sets the target width for icon images.
    /// </summary>
    public int? IconWidth { get; set; }

    /// <summary>
    /// Gets or sets the target height for icon images.
    /// </summary>
    public int? IconHeight { get; set; }

    /// <summary>
    /// Gets or sets the target width for logo images.
    /// </summary>
    public int? LogoWidth { get; set; }

    /// <summary>
    /// Gets or sets the target height for logo images.
    /// </summary>
    public int? LogoHeight { get; set; }

    /// <summary>
    /// Gets or sets the target width for title images.
    /// </summary>
    public int? TitleWidth { get; set; }

    /// <summary>
    /// Gets or sets the target height for title images.
    /// </summary>
    public int? TitleHeight { get; set; }

    /// <summary>
    /// Gets or sets the target width for hero images.
    /// </summary>
    public int? HeroWidth { get; set; }

    /// <summary>
    /// Gets or sets the target height for hero images.
    /// </summary>
    public int? HeroHeight { get; set; }

    /// <summary>
    /// Gets or sets the target width for slide images.
    /// </summary>
    public int? SlideWidth { get; set; }

    /// <summary>
    /// Gets or sets the target height for slide images.
    /// </summary>
    public int? SlideHeight { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration for music tracks.
    /// </summary>
    public TimeSpan? MaxMusicDuration { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration for music search results.
    /// Tracks longer than this are excluded from search results.
    /// </summary>
    public TimeSpan? SearchMusicMaxDuration { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated priority terms for music search.
    /// </summary>
    public string? MusicSearchTermPriority { get; set; }

    /// <summary>
    /// Gets or sets whether to sort music by like count.
    /// </summary>
    public bool IsSortMusicByLikes { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration for videos.
    /// </summary>
    public TimeSpan? MaxVideoDuration { get; set; }

    /// <summary>
    /// Gets or sets the preferred video quality.
    /// </summary>
    public string? VideoQuality { get; set; }

    /// <summary>
    /// Gets or sets the folder name configurations.
    /// </summary>
    public List<FolderNameConfiguration> FolderConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the game icon overlay configurations.
    /// </summary>
    public List<GameIconOverlayConfiguration> GameIconOverlayConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the platform icon configurations.
    /// </summary>
    public List<PlatformIconConfiguration> PlatformIconConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the file extension configurations for each platform.
    /// </summary>
    public List<ExtensionConfiguration> ExtensionConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the scraper configurations.
    /// </summary>
    public List<ScraperConfiguration> ScraperConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the upscaler configurations.
    /// </summary>
    public List<UpscalerConfiguration> UpscalerConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the platform configurations.
    /// </summary>
    public List<PlatformConfiguration> PlatformConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of specific platforms to process.
    /// </summary>
    public List<string> SpecificPlatforms { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of specific games to process.
    /// </summary>
    public List<SpecificGame> SpecificGames { get; set; } = [];
}
