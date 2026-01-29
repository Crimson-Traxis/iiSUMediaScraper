using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservableConfiguration : BaseObservableModel<Configuration>
{
    public ObservableConfiguration(Configuration baseModel) : base(baseModel)
    {
    }

    public string? GamesPath
    {
        get => _baseModel.GamesPath;
        set => SetProperty(_baseModel.GamesPath, value, _baseModel, (o, v) => o.GamesPath = v);
    }

    public string? ApplyAssetPath
    {
        get => _baseModel.ApplyAssetPath;
        set => SetProperty(_baseModel.ApplyAssetPath, value, _baseModel, (o, v) => o.ApplyAssetPath = v);
    }

    public string? UnfoundMediaMovePath
    {
        get => _baseModel.UnfoundMediaMovePath;
        set => SetProperty(_baseModel.UnfoundMediaMovePath, value, _baseModel, (o, v) => o.UnfoundMediaMovePath = v);
    }

    public string? SteamGridDbKey
    {
        get => _baseModel.SteamGridDbKey;
        set => SetProperty(_baseModel.SteamGridDbKey, value, _baseModel, (o, v) => o.SteamGridDbKey = v);
    }

    public string? IgdbClientSecret
    {
        get => _baseModel.IgdbClientSecret;
        set => SetProperty(_baseModel.IgdbClientSecret, value, _baseModel, (o, v) => o.IgdbClientSecret = v);
    }

    public string? IgdbClientId
    {
        get => _baseModel.IgdbClientId;
        set => SetProperty(_baseModel.IgdbClientId, value, _baseModel, (o, v) => o.IgdbClientId = v);
    }

    public bool IsUseLogoAsTitle
    {
        get => _baseModel.IsUseLogoAsTitle;
        set => SetProperty(_baseModel.IsUseLogoAsTitle, value, _baseModel, (o, v) => o.IsUseLogoAsTitle = v);
    }

    public bool IsPrioritizeLogoAsTitle
    {
        get => _baseModel.IsPrioritizeLogoAsTitle;
        set => SetProperty(_baseModel.IsPrioritizeLogoAsTitle, value, _baseModel, (o, v) => o.IsPrioritizeLogoAsTitle = v);
    }

    public string? DefaultUpscaleConfigurationName
    {
        get => _baseModel.DefaultUpscaleConfigurationName;
        set => SetProperty(_baseModel.DefaultUpscaleConfigurationName, value, _baseModel, (o, v) => o.DefaultUpscaleConfigurationName = v);
    }

    public string? DefaultReconstructorConfigurationName
    {
        get => _baseModel.DefaultReconstructorConfigurationName;
        set => SetProperty(_baseModel.DefaultReconstructorConfigurationName, value, _baseModel, (o, v) => o.DefaultReconstructorConfigurationName = v);
    }

    public bool IsScanGames
    {
        get => _baseModel.IsScanGames;
        set => SetProperty(_baseModel.IsScanGames, value, _baseModel, (o, v) => o.IsScanGames = v);
    }

    public bool IsScanUnfoundGames
    {
        get => _baseModel.IsScanUnfoundGames;
        set => SetProperty(_baseModel.IsScanUnfoundGames, value, _baseModel, (o, v) => o.IsScanUnfoundGames = v);
    }

    public bool IsUnfoundMediaIfNoIcons
    {
        get => _baseModel.IsUnfoundMediaIfNoIcons;
        set => SetProperty(_baseModel.IsUnfoundMediaIfNoIcons, value, _baseModel, (o, v) => o.IsUnfoundMediaIfNoIcons = v);
    }

    public bool IsUnfoundMediaIfNoLogos
    {
        get => _baseModel.IsUnfoundMediaIfNoLogos;
        set => SetProperty(_baseModel.IsUnfoundMediaIfNoLogos, value, _baseModel, (o, v) => o.IsUnfoundMediaIfNoLogos = v);
    }

    public bool IsUnfoundMediaIfNoTitles
    {
        get => _baseModel.IsUnfoundMediaIfNoTitles;
        set => SetProperty(_baseModel.IsUnfoundMediaIfNoTitles, value, _baseModel, (o, v) => o.IsUnfoundMediaIfNoTitles = v);
    }

    public bool IsUnfoundMediaIfNoHeros
    {
        get => _baseModel.IsUnfoundMediaIfNoHeros;
        set => SetProperty(_baseModel.IsUnfoundMediaIfNoHeros, value, _baseModel, (o, v) => o.IsUnfoundMediaIfNoHeros = v);
    }

    public bool IsUnfoundMediaIfNoSlides
    {
        get => _baseModel.IsUnfoundMediaIfNoSlides;
        set => SetProperty(_baseModel.IsUnfoundMediaIfNoSlides, value, _baseModel, (o, v) => o.IsUnfoundMediaIfNoSlides = v);
    }

    public bool IsMoveToUnfoundGamesFolder
    {
        get => _baseModel.IsMoveToUnfoundGamesFolder;
        set => SetProperty(_baseModel.IsMoveToUnfoundGamesFolder, value, _baseModel, (o, v) => o.IsMoveToUnfoundGamesFolder = v);
    }

    public bool IsApplyIcon
    {
        get => _baseModel.IsApplyIcon;
        set => SetProperty(_baseModel.IsApplyIcon, value, _baseModel, (o, v) => o.IsApplyIcon = v);
    }

    public bool IsApplyTitle
    {
        get => _baseModel.IsApplyTitle;
        set => SetProperty(_baseModel.IsApplyTitle, value, _baseModel, (o, v) => o.IsApplyTitle = v);
    }

    public bool IsApplyHeros
    {
        get => _baseModel.IsApplyHeros;
        set => SetProperty(_baseModel.IsApplyHeros, value, _baseModel, (o, v) => o.IsApplyHeros = v);
    }

    public bool IsApplySlides
    {
        get => _baseModel.IsApplySlides;
        set => SetProperty(_baseModel.IsApplySlides, value, _baseModel, (o, v) => o.IsApplySlides = v);
    }

    public string IconNameFormat
    {
        get => _baseModel.IconNameFormat;
        set => SetProperty(_baseModel.IconNameFormat, value, _baseModel, (o, v) => o.IconNameFormat = v);
    }

    public string TitleNameFormat
    {
        get => _baseModel.TitleNameFormat;
        set => SetProperty(_baseModel.TitleNameFormat, value, _baseModel, (o, v) => o.TitleNameFormat = v);
    }

    public string HeroNameFormat
    {
        get => _baseModel.HeroNameFormat;
        set => SetProperty(_baseModel.HeroNameFormat, value, _baseModel, (o, v) => o.HeroNameFormat = v);
    }

    public string SlideNameFormat
    {
        get => _baseModel.SlideNameFormat;
        set => SetProperty(_baseModel.SlideNameFormat, value, _baseModel, (o, v) => o.SlideNameFormat = v);
    }

    public int? MaxNumberOfConcurrentGames
    {
        get => _baseModel.MaxNumberOfConcurrentGames;
        set => SetProperty(_baseModel.MaxNumberOfConcurrentGames, value, _baseModel, (o, v) => o.MaxNumberOfConcurrentGames = v);
    }

    public bool IsApplyUnfoundGames
    {
        get => _baseModel.IsApplyUnfoundGames;
        set => SetProperty(_baseModel.IsApplyUnfoundGames, value, _baseModel, (o, v) => o.IsApplyUnfoundGames = v);
    }

    public bool IsDeleteExistingHeroAssets
    {
        get => _baseModel.IsDeleteExistingHeroAssets;
        set => SetProperty(_baseModel.IsDeleteExistingHeroAssets, value, _baseModel, (o, v) => o.IsDeleteExistingHeroAssets = v);
    }

    public bool IsDeleteExistingSlideAssets
    {
        get => _baseModel.IsDeleteExistingSlideAssets;
        set => SetProperty(_baseModel.IsDeleteExistingSlideAssets, value, _baseModel, (o, v) => o.IsDeleteExistingSlideAssets = v);
    }

    public int? IconWidth
    {
        get => _baseModel.IconWidth;
        set => SetProperty(_baseModel.IconWidth, value, _baseModel, (o, v) => o.IconWidth = v);
    }

    public int? IconHeight
    {
        get => _baseModel.IconHeight;
        set => SetProperty(_baseModel.IconHeight, value, _baseModel, (o, v) => o.IconHeight = v);
    }

    public int? LogoWidth
    {
        get => _baseModel.LogoWidth;
        set => SetProperty(_baseModel.LogoWidth, value, _baseModel, (o, v) => o.LogoWidth = v);
    }

    public int? LogoHeight
    {
        get => _baseModel.LogoHeight;
        set => SetProperty(_baseModel.LogoHeight, value, _baseModel, (o, v) => o.LogoHeight = v);
    }

    public int? TitleWidth
    {
        get => _baseModel.TitleWidth;
        set => SetProperty(_baseModel.TitleWidth, value, _baseModel, (o, v) => o.TitleWidth = v);
    }


    public int? TitleHeight
    {
        get => _baseModel.TitleHeight;
        set => SetProperty(_baseModel.TitleHeight, value, _baseModel, (o, v) => o.TitleHeight = v);
    }

    public int? HeroWidth
    {
        get => _baseModel.HeroWidth;
        set => SetProperty(_baseModel.HeroWidth, value, _baseModel, (o, v) => o.HeroWidth = v);
    }

    public int? HeroHeight
    {
        get => _baseModel.HeroHeight;
        set => SetProperty(_baseModel.HeroHeight, value, _baseModel, (o, v) => o.HeroHeight = v);
    }

    public int? SlideWidth
    {
        get => _baseModel.SlideWidth;
        set => SetProperty(_baseModel.SlideWidth, value, _baseModel, (o, v) => o.SlideWidth = v);
    }

    public int? SlideHeight
    {
        get => _baseModel.SlideHeight;
        set => SetProperty(_baseModel.SlideHeight, value, _baseModel, (o, v) => o.SlideHeight = v);
    }

    public List<FolderNameConfiguration> FolderConfigurations
    {
        get => _baseModel.FolderConfigurations;
        set => SetProperty(_baseModel.FolderConfigurations, value, _baseModel, (o, v) => o.FolderConfigurations = v);
    }

    public List<GameIconOverlayConfiguration> IconOverlayConfigurations
    {
        get => _baseModel.GameIconOverlayConfigurations;
        set => SetProperty(_baseModel.GameIconOverlayConfigurations, value, _baseModel, (o, v) => o.GameIconOverlayConfigurations = v);
    }

    public List<PlatformIconConfiguration> PlatformIconConfigurations
    {
        get => _baseModel.PlatformIconConfigurations;
        set => SetProperty(_baseModel.PlatformIconConfigurations, value, _baseModel, (o, v) => o.PlatformIconConfigurations = v);
    }

    public List<ExtensionConfiguration> ExtensionConfigurations
    {
        get => _baseModel.ExtensionConfigurations;
        set => SetProperty(_baseModel.ExtensionConfigurations, value, _baseModel, (o, v) => o.ExtensionConfigurations = v);
    }

    public List<ScraperConfiguration> ScraperConfigurations
    {
        get => _baseModel.ScraperConfigurations;
        set => SetProperty(_baseModel.ScraperConfigurations, value, _baseModel, (o, v) => o.ScraperConfigurations = v);
    }

    public List<UpscalerConfiguration> UpscalerConfigurations
    {
        get => _baseModel.UpscalerConfigurations;
        set => SetProperty(_baseModel.UpscalerConfigurations, value, _baseModel, (o, v) => o.UpscalerConfigurations = v);
    }

    public List<PlatformConfiguration> PlatformConfigurations
    {
        get => _baseModel.PlatformConfigurations;
        set => SetProperty(_baseModel.PlatformConfigurations, value, _baseModel, (o, v) => o.PlatformConfigurations = v);
    }

    //public List<UpscalerConfiguration> ReconstructorConfigurations
    //{
    //    get => _baseModel.ReconstructorConfigurations;
    //    set => SetProperty(_baseModel.ReconstructorConfigurations, value, _baseModel, (o, v) => o.ReconstructorConfigurations = v);
    //}

    public List<string> SpecificPlatforms
    {
        get => _baseModel.SpecificPlatforms;
        set => SetProperty(_baseModel.SpecificPlatforms, value, _baseModel, (o, v) => o.SpecificPlatforms = v);
    }

    public List<SpecificGame> SpecificGames
    {
        get => _baseModel.SpecificGames;
        set => SetProperty(_baseModel.SpecificGames, value, _baseModel, (o, v) => o.SpecificGames = v);
    }
}
