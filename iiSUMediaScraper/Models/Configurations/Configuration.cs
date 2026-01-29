namespace iiSUMediaScraper.Models.Configurations;

public class Configuration
{
    public string? GamesPath { get; set; }

    public string? ApplyAssetPath { get; set; }

    public string? UnfoundMediaMovePath { get; set; }

    public string? SteamGridDbKey { get; set; }

    public string? IgdbClientSecret { get; set; }

    public string? IgdbClientId { get; set; }

    public bool IsPrioritizeLogoAsTitle { get; set; }

    public bool IsUseLogoAsTitle { get; set; }

    public bool IsMoveToUnfoundGamesFolder { get; set; }

    public bool IsApplyIcon { get; set; }

    public bool IsApplyTitle { get; set; }

    public bool IsApplyHeros { get; set; }

    public bool IsApplySlides { get; set; }

    public string? IconNameFormat { get; set; }

    public string? TitleNameFormat { get; set; }

    public string? HeroNameFormat { get; set; }

    public string? SlideNameFormat { get; set; }

    public bool IsApplyUnfoundGames { get; set; }

    public bool IsDeleteExistingHeroAssets { get; set; }

    public bool IsDeleteExistingSlideAssets { get; set; }

    public int? MaxNumberOfConcurrentGames { get; set; }

    public string? DefaultUpscaleConfigurationName { get; set; }

    public string? DefaultReconstructorConfigurationName { get; set; }

    public bool IsScanGames { get; set; }

    public bool IsScanUnfoundGames { get; set; }

    public bool IsUnfoundMediaIfNoIcons { get; set; }

    public bool IsUnfoundMediaIfNoLogos { get; set; }

    public bool IsUnfoundMediaIfNoTitles { get; set; }

    public bool IsUnfoundMediaIfNoHeros { get; set; }

    public bool IsUnfoundMediaIfNoSlides { get; set; }

    public int? IconWidth { get; set; }

    public int? IconHeight { get; set; }

    public int? LogoWidth { get; set; }

    public int? LogoHeight { get; set; }

    public int? TitleWidth { get; set; }

    public int? TitleHeight { get; set; }

    public int? HeroWidth { get; set; }

    public int? HeroHeight { get; set; }

    public int? SlideWidth { get; set; }

    public int? SlideHeight { get; set; }

    public List<FolderNameConfiguration> FolderConfigurations { get; set; } = [];

    public List<GameIconOverlayConfiguration> GameIconOverlayConfigurations { get; set; } = [];

    public List<PlatformIconConfiguration> PlatformIconConfigurations { get; set; } = [];

    public List<ExtensionConfiguration> ExtensionConfigurations { get; set; } = [];

    public List<ScraperConfiguration> ScraperConfigurations { get; set; } = [];

    public List<UpscalerConfiguration> UpscalerConfigurations { get; set; } = [];

    public List<PlatformConfiguration> PlatformConfigurations { get; set; } = [];

    public List<string> SpecificPlatforms { get; set; } = [];

    public List<SpecificGame> SpecificGames { get; set; } = [];
}
