using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using iiSUMediaScraper.ViewModels.Configurations;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for application configuration settings.
/// </summary>
public partial class ConfigurationViewModel : ObservableConfiguration
{
    /// <summary>
    /// Raised when removal is requested for the configuration.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Gets or sets the width for icon images.
    /// </summary>
    [ObservableProperty]
    private double iconWidth = double.NaN;

    /// <summary>
    /// Gets or sets the height for icon images.
    /// </summary>
    [ObservableProperty]
    private double iconHeight = double.NaN;

    /// <summary>
    /// Gets or sets the width for logo images.
    /// </summary>
    [ObservableProperty]
    private double logoWidth = double.NaN;

    /// <summary>
    /// Gets or sets the height for logo images.
    /// </summary>
    [ObservableProperty]
    private double logoHeight = double.NaN;

    /// <summary>
    /// Gets or sets the width for title images.
    /// </summary>
    [ObservableProperty]
    private double titleWidth = double.NaN;

    /// <summary>
    /// Gets or sets the height for title images.
    /// </summary>
    [ObservableProperty]
    private double titleHeight = double.NaN;

    /// <summary>
    /// Gets or sets the width for hero images.
    /// </summary>
    [ObservableProperty]
    private double heroWidth = double.NaN;

    /// <summary>
    /// Gets or sets the height for hero images.
    /// </summary>
    [ObservableProperty]
    private double heroHeight = double.NaN;

    /// <summary>
    /// Gets or sets the width for slide images.
    /// </summary>
    [ObservableProperty]
    private double slideWidth = double.NaN;

    /// <summary>
    /// Gets or sets the height for slide images.
    /// </summary>
    [ObservableProperty]
    private double slideHeight = double.NaN;

    /// <summary>
    /// Max Duration of the music to download.
    /// </summary>
    [ObservableProperty]
    private double maxMusicDuration = double.NaN;

    /// <summary>
    /// Max duration for music search results.
    /// </summary>
    [ObservableProperty]
    private double searchMusicMaxDuration = double.NaN;

    /// <summary>
    /// Max Duration of the video to download.
    /// </summary>
    [ObservableProperty]
    private double maxVideoDuration = double.NaN;

    /// <summary>
    /// Gets or sets the maximum number of concurrent games to process.
    /// </summary>
    [ObservableProperty]
    private double maxNumberOfConcurrentGames = double.NaN;

    /// <summary>
    /// Gets or sets the search text for filtering specific games.
    /// </summary>
    [ObservableProperty]
    private string specificGamesSearch;

    /// <summary>
    /// Gets or sets the search text for filtering platform definition configurations.
    /// </summary>
    [ObservableProperty]
    private string platformDefinitionConfigurationsSearch;

    /// <summary>
    /// Gets or sets the search text for filtering specific platforms.
    /// </summary>
    [ObservableProperty]
    private string specificPlatformsSearch;

    /// <summary>
    /// Gets or sets the search text for filtering image upscaler configurations.
    /// </summary>
    [ObservableProperty]
    private string imageUpscalerConfigurationsSearch;

    /// <summary>
    /// Gets or sets the search text for filtering image reconstructor configurations.
    /// </summary>
    [ObservableProperty]
    private string imageReconstructorConfigurationsSearch;

    /// <summary>
    /// Gets or sets the search text for filtering folder configurations.
    /// </summary>
    [ObservableProperty]
    private string folderConfigurationsSearch;

    /// <summary>
    /// Gets or sets the search text for filtering game icon overlay configurations.
    /// </summary>
    [ObservableProperty]
    private string gameIconOverlayConfigurationsSearch;

    /// <summary>
    /// Gets or sets the search text for filtering platform icon configurations.
    /// </summary>
    [ObservableProperty]
    private string platformIconConfigurationsSearch;

    /// <summary>
    /// Gets or sets the search text for filtering extension configurations.
    /// </summary>
    [ObservableProperty]
    private string extensionConfigurationsSearch;

    /// <summary>
    /// Gets or sets the selected upscaler configuration.
    /// </summary>
    [ObservableProperty]
    private UpscalerConfigurationViewModel? selectedUpscalerConfiguration;

    /// <summary>
    /// Gets or sets the selected reconstructor configuration.
    /// </summary>
    [ObservableProperty]
    private UpscalerConfigurationViewModel? selectedReconstructorConfiguration;

    /// <summary>
    /// Gets or sets the collection of folder name configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FolderNameConfigurationViewModel> folderConfigurations;

    /// <summary>
    /// Gets or sets the collection of game icon overlay configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<GameIconOverlayConfigurationViewModel> gameIconOverlayConfigurations;

    /// <summary>
    /// Gets or sets the collection of platform icon configurations.
    /// </summary>
    [ObservableProperty]
    public ObservableCollection<PlatformIconConfigurationViewModel> platformIconConfigurations;

    /// <summary>
    /// Gets or sets the collection of extension configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ExtensionConfigurationViewModel> extensionConfigurations;

    /// <summary>
    /// Gets or sets the collection of scraper configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ScraperConfigurationViewModel> scraperConfigurations;

    /// <summary>
    /// Gets or sets the collection of upscaler configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<UpscalerConfigurationViewModel> upscalerConfigurations;

    /// <summary>
    /// Gets or sets the collection of platform configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PlatformConfigurationViewModel> platformConfigurations;

    /// <summary>
    /// Gets or sets the collection of reconstructor configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<UpscalerConfigurationViewModel> reconstructorConfigurations;

    /// <summary>
    /// Gets or sets the collection of specific platforms.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SpecificPlatformViewModel> specificPlatforms;

    /// <summary>
    /// Gets or sets the collection of specific games.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SpecificGameViewModel> specificGames;

    /// <summary>
    /// Gets or sets the collection of games path history entries.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PathHistoryViewModel> gamesPathHistory;

    /// <summary>
    /// Gets or sets the collection of apply asset path history entries.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PathHistoryViewModel> applyAssetPathHistory;

    /// <summary>
    /// Gets or sets the collection of unfound media move path history entries.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PathHistoryViewModel> unfoundMediaMovePathHistory;

    /// <summary>
    /// Gets the available video quality options for yt-dlp.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> videoQualities;

    /// <summary>
    /// Gets or sets the collection of selected specific platforms.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SpecificPlatformViewModel> selectedSpecificPlatforms;

    /// <summary>
    /// Raised when showing the configuration dialog is requested.
    /// </summary>
    public event EventHandler? ShowConfiguraitonRequested;

    /// <summary>
    /// Raised when the specific games search text changes.
    /// </summary>
    public event EventHandler? SpecificGamesSearchChanged;

    /// <summary>
    /// Raised when the platform definition configurations search text changes.
    /// </summary>
    public event EventHandler? PlatformDefinitionConfigurationsSearchChanged;

    /// <summary>
    /// Raised when the specific platforms search text changes.
    /// </summary>
    public event EventHandler? SpecificPlatformsSearchChanged;

    /// <summary>
    /// Raised when the image upscaler configurations search text changes.
    /// </summary>
    public event EventHandler? ImageUpscalerConfigurationsSearchChanged;

    /// <summary>
    /// Raised when the image reconstructor configurations search text changes.
    /// </summary>
    public event EventHandler? ImageReconstructorConfigurationsSearchChanged;

    /// <summary>
    /// Raised when the folder configurations search text changes.
    /// </summary>
    public event EventHandler? FolderConfigurationsSearchChanged;

    /// <summary>
    /// Raised when the game icon overlay configurations search text changes.
    /// </summary>
    public event EventHandler? GameIconOverlayConfigurationsSearchChanged;

    /// <summary>
    /// Raised when the platform icon configurations search text changes.
    /// </summary>
    public event EventHandler? PlatformIconConfigurationsSearchChanged;

    /// <summary>
    /// Raised when the extension configurations search text changes.
    /// </summary>
    public event EventHandler? ExtensionConfigurationsSearchChanged;

    /// <summary>
    /// Raised when a games path history entry is removed.
    /// </summary>
    public event EventHandler? GamesPathHistoryRemoved;

    /// <summary>
    /// Raised when an apply asset path history entry is removed.
    /// </summary>
    public event EventHandler? ApplyAssetPathHistoryRemoved;

    /// <summary>
    /// Raised when an unfound media move path history entry is removed.
    /// </summary>
    public event EventHandler? UnfoundMediaMovePathHistoryRemoved;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying configuration model.</param>
    /// <param name="fileService">The file service.</param>
    public ConfigurationViewModel(Configuration baseModel, IFileService fileService) : base(baseModel)
    {
        FileService = fileService;

        if (base.IconWidth != null)
        {
            IconWidth = (int)base.IconWidth;
        }

        if (base.IconHeight != null)
        {
            IconHeight = (int)base.IconHeight;
        }

        if (base.LogoWidth != null)
        {
            LogoWidth = (int)base.LogoWidth;
        }

        if (base.LogoHeight != null)
        {
            LogoHeight = (int)base.LogoHeight;
        }

        if (base.TitleWidth != null)
        {
            TitleWidth = (int)base.TitleWidth;
        }

        if (base.TitleHeight != null)
        {
            TitleHeight = (int)base.TitleHeight;
        }

        if (base.HeroWidth != null)
        {
            HeroWidth = (int)base.HeroWidth;
        }

        if (base.HeroHeight != null)
        {
            HeroHeight = (int)base.HeroHeight;
        }

        if (base.SlideWidth != null)
        {
            SlideWidth = (int)base.SlideWidth;
        }

        if (base.SlideHeight != null)
        {
            SlideHeight = (int)base.SlideHeight;
        }

        if (base.MaxNumberOfConcurrentGames != null)
        {
            MaxNumberOfConcurrentGames = (int)base.MaxNumberOfConcurrentGames;
        }

        if (base.MaxMusicDuration != null)
        {
            MaxMusicDuration = base.MaxMusicDuration.Value.TotalSeconds;
        }

        if (base.SearchMusicMaxDuration != null)
        {
            SearchMusicMaxDuration = base.SearchMusicMaxDuration.Value.TotalSeconds;
        }

        if (base.MaxVideoDuration != null)
        {
            MaxVideoDuration = base.MaxVideoDuration.Value.TotalSeconds;
        }

        folderConfigurations = [];
        gameIconOverlayConfigurations = [];
        platformIconConfigurations = [];
        extensionConfigurations = [];
        scraperConfigurations = [];
        upscalerConfigurations = [];
        platformConfigurations = [];
        reconstructorConfigurations = [];
        specificPlatforms = [];
        specificGames = [];
        gamesPathHistory = [];
        applyAssetPathHistory = [];
        unfoundMediaMovePathHistory = [];

        selectedSpecificPlatforms = [];

        videoQualities = 
        [
            "best",
            "bestvideo+bestaudio/best",
            "1080p",
            "720p",
            "480p",
            "360p",
            "worst"
        ];

        VideoQuality ??= "bestvideo+bestaudio/best";

        // Register PlatformConfigurations first since other collections reference it
        RegisterBaseModelObservableCollection(
            nameof(PlatformConfigurations),
            baseModel.PlatformConfigurations,
            platformConfigurations,
            CreatePlatformConfiguration,
            InitializePlatformConfiguration);

        RegisterBaseModelObservableCollection(
            nameof(FolderConfigurations),
            baseModel.FolderConfigurations,
            folderConfigurations,
            CreateFolderNameConfiguration,
            InitializeFolderNameConfiguration);

        RegisterBaseModelObservableCollection(
            nameof(GameIconOverlayConfigurations),
            baseModel.GameIconOverlayConfigurations,
            gameIconOverlayConfigurations,
            CreateGameIconOverlayConfiguration,
            InitializeGameIconOverlayConfiguration);

        RegisterBaseModelObservableCollection(
            nameof(PlatformIconConfigurations),
            baseModel.PlatformIconConfigurations,
            platformIconConfigurations,
            CreatePlatformIconConfiguration,
            InitializePlatformIconConfiguration);

        RegisterBaseModelObservableCollection(
            nameof(ExtensionConfigurations),
            baseModel.ExtensionConfigurations,
            extensionConfigurations,
            CreateExtensionConfiguration,
            InitializeExtensionConfiguration);

        RegisterBaseModelObservableCollection(
            nameof(ScraperConfigurations),
            baseModel.ScraperConfigurations,
            scraperConfigurations,
            CreateScraperConfiguration,
            InitializeScraperConfiguration);

        RegisterBaseModelObservableCollection(
            nameof(UpscalerConfigurations),
            baseModel.UpscalerConfigurations,
            upscalerConfigurations,
            CreateUpscalerConfiguration,
            InitializeUpscalerConfiguration);

        RegisterObservableCollection(
            nameof(GamesPathHistory),
            baseModel.GamesPathHistory,
            gamesPathHistory,
            path => new PathHistoryViewModel(path),
            InitializePathHistory,
            vm => vm.Path);

        RegisterObservableCollection(
            nameof(ApplyAssetPathHistory),
            baseModel.ApplyAssetPathHistory,
            applyAssetPathHistory,
            path => new PathHistoryViewModel(path),
            InitializePathHistory,
            vm => vm.Path);

        RegisterObservableCollection(
            nameof(UnfoundMediaMovePathHistory),
            baseModel.UnfoundMediaMovePathHistory,
            unfoundMediaMovePathHistory,
            path => new PathHistoryViewModel(path),
            InitializePathHistory,
            vm => vm.Path);

        selectedUpscalerConfiguration ??= upscalerConfigurations.FirstOrDefault();

        selectedReconstructorConfiguration ??= reconstructorConfigurations.FirstOrDefault();

        specificPlatforms.CollectionChanged += delegate { UpdateSpecificPlatforms(); };

        UpdateSpecificPlatforms();

        platformIconConfigurations.CollectionChanged += delegate { UpdateSpecificPlatformsIconPaths(); };

        UpdateSpecificPlatformsIconPaths();

        UpdateSpecificGames();

        UpdateSelectedSpecificPlatforms();
    }

    /// <summary>
    /// Called when the specific games search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnSpecificGamesSearchChanged(string value)
    {
        SpecificGamesSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the platform definition configurations search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnPlatformDefinitionConfigurationsSearchChanged(string value)
    {
        PlatformDefinitionConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the specific platforms search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnSpecificPlatformsSearchChanged(string value)
    {
        SpecificPlatformsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the image upscaler configurations search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnImageUpscalerConfigurationsSearchChanged(string value)
    {
        ImageUpscalerConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the image reconstructor configurations search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnImageReconstructorConfigurationsSearchChanged(string value)
    {
        ImageReconstructorConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the folder configurations search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnFolderConfigurationsSearchChanged(string value)
    {
        FolderConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the game icon overlay configurations search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnGameIconOverlayConfigurationsSearchChanged(string value)
    {
        GameIconOverlayConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the platform icon configurations search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnPlatformIconConfigurationsSearchChanged(string value)
    {
        PlatformIconConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the extension configurations search text changes.
    /// </summary>
    /// <param name="value">The new search text.</param>
    partial void OnExtensionConfigurationsSearchChanged(string value)
    {
        ExtensionConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the maximum number of concurrent games changes to update the base model.
    /// </summary>
    /// <param name="value">The new value.</param>
    partial void OnMaxNumberOfConcurrentGamesChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.MaxNumberOfConcurrentGames = null;
        }
        else
        {
            base.MaxNumberOfConcurrentGames = (int)value;
        }
    }

    /// <summary>
    /// Called when the icon width changes to update the base model.
    /// </summary>
    /// <param name="value">The new width value.</param>
    partial void OnIconWidthChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.IconWidth = null;
        }
        else
        {
            base.IconWidth = (int)value;
        }
    }

    /// <summary>
    /// Called when the icon height changes to update the base model.
    /// </summary>
    /// <param name="value">The new height value.</param>
    partial void OnIconHeightChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.IconHeight = null;
        }
        else
        {
            base.IconHeight = (int)value;
        }
    }

    /// <summary>
    /// Called when the logo width changes to update the base model.
    /// </summary>
    /// <param name="value">The new width value.</param>
    partial void OnLogoWidthChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.LogoWidth = null;
        }
        else
        {
            base.LogoWidth = (int)value;
        }
    }

    /// <summary>
    /// Called when the logo height changes to update the base model.
    /// </summary>
    /// <param name="value">The new height value.</param>
    partial void OnLogoHeightChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.LogoHeight = null;
        }
        else
        {
            base.LogoHeight = (int)value;
        }
    }

    /// <summary>
    /// Called when the title width changes to update the base model.
    /// </summary>
    /// <param name="value">The new width value.</param>
    partial void OnTitleWidthChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.TitleWidth = null;
        }
        else
        {
            base.TitleWidth = (int)value;
        }
    }

    /// <summary>
    /// Called when the title height changes to update the base model.
    /// </summary>
    /// <param name="value">The new height value.</param>
    partial void OnTitleHeightChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.TitleHeight = null;
        }
        else
        {
            base.TitleHeight = (int)value;
        }
    }

    /// <summary>
    /// Called when the hero width changes to update the base model.
    /// </summary>
    /// <param name="value">The new width value.</param>
    partial void OnHeroWidthChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.HeroWidth = null;
        }
        else
        {
            base.HeroWidth = (int)value;
        }
    }

    /// <summary>
    /// Called when the hero height changes to update the base model.
    /// </summary>
    /// <param name="value">The new height value.</param>
    partial void OnHeroHeightChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.HeroHeight = null;
        }
        else
        {
            base.HeroHeight = (int)value;
        }
    }

    /// <summary>
    /// Called when the slide height changes to update the base model.
    /// </summary>
    /// <param name="value">The new height value.</param>
    partial void OnSlideHeightChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.SlideHeight = null;
        }
        else
        {
            base.SlideHeight = (int)value;
        }
    }

    /// <summary>
    /// Called when the max music duration changes to update the base model.
    /// </summary>
    /// <param name="value">The new duration in seconds.</param>
    partial void OnMaxMusicDurationChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.MaxMusicDuration = null;
        }
        else
        {
            base.MaxMusicDuration = TimeSpan.FromSeconds(value);
        }
    }

    /// <summary>
    /// Called when the search music max duration changes to update the base model.
    /// </summary>
    /// <param name="value">The new duration in seconds.</param>
    partial void OnSearchMusicMaxDurationChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.SearchMusicMaxDuration = null;
        }
        else
        {
            base.SearchMusicMaxDuration = TimeSpan.FromSeconds(value);
        }
    }

    /// <summary>
    /// Called when the max video duration changes to update the base model.
    /// </summary>
    /// <param name="value">The new duration in seconds.</param>
    partial void OnMaxVideoDurationChanged(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            base.MaxVideoDuration = null;
        }
        else
        {
            base.MaxVideoDuration = TimeSpan.FromSeconds(value);
        }
    }

    /// <summary>
    /// Called when the selected upscaler configuration changes to update the base model.
    /// </summary>
    /// <param name="value">The new selected upscaler configuration.</param>
    partial void OnSelectedUpscalerConfigurationChanged(UpscalerConfigurationViewModel? value)
    {
        base.DefaultUpscaleConfigurationName = value?.Name;
    }

    /// <summary>
    /// Called when the selected reconstructor configuration changes to update the base model.
    /// </summary>
    /// <param name="value">The new selected reconstructor configuration.</param>
    partial void OnSelectedReconstructorConfigurationChanged(UpscalerConfigurationViewModel? value)
    {
        base.DefaultReconstructorConfigurationName = value?.Name;
    }

    /// <summary>
    /// Handles the remove requested event for a folder name configuration.
    /// </summary>
    /// <param name="sender">The folder name configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnFolderNameConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is FolderNameConfigurationViewModel item)
        {
            RemoveFolderNameConfiguration(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a game icon overlay configuration.
    /// </summary>
    /// <param name="sender">The game icon overlay configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnGameIconOverlayConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is GameIconOverlayConfigurationViewModel item)
        {
            RemoveGameIconOverlayConfiguration(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a platform icon configuration.
    /// </summary>
    /// <param name="sender">The platform icon configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPlatformIconConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is PlatformIconConfigurationViewModel item)
        {
            RemovePlatformIconConfiguration(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for an extension configuration.
    /// </summary>
    /// <param name="sender">The extension configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnExtensionConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ExtensionConfigurationViewModel item)
        {
            RemoveExtensionConfiguration(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a scraper configuration.
    /// </summary>
    /// <param name="sender">The scraper configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnScraperConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ScraperConfigurationViewModel item)
        {
            RemoveScraperConfiguration(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for an upscaler configuration.
    /// </summary>
    /// <param name="sender">The upscaler configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnUpscalerConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is UpscalerConfigurationViewModel item)
        {
            RemoveUpscalerConfiguration(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a platform configuration.
    /// </summary>
    /// <param name="sender">The platform configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPlatformConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is PlatformConfigurationViewModel item)
        {
            RemovePlatformConfiguration(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a reconstructor configuration.
    /// </summary>
    /// <param name="sender">The reconstructor configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnReconstructorConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is UpscalerConfigurationViewModel item)
        {
            RemoveReconstructorConfiguration(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a specific game.
    /// </summary>
    /// <param name="sender">The specific game requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnSpecificGameRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is SpecificGameViewModel item)
        {
            RemoveSpecificGame(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a path history entry.
    /// </summary>
    /// <param name="sender">The path history entry requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPathHistoryRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is PathHistoryViewModel item)
        {
            if (GamesPathHistory.Remove(item))
                GamesPathHistoryRemoved?.Invoke(this, EventArgs.Empty);

            if (ApplyAssetPathHistory.Remove(item))
                ApplyAssetPathHistoryRemoved?.Invoke(this, EventArgs.Empty);

            if (UnfoundMediaMovePathHistory.Remove(item))
                UnfoundMediaMovePathHistoryRemoved?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Handles the selection changed event for a specific platform.
    /// </summary>
    /// <param name="sender">The specific platform whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void SpecificPlatform_SelectedChanged(object? sender, EventArgs e)
    {
        UpdateBaseSpecificPlatforms();

        UpdateSelectedSpecificPlatforms();
    }

    /// <summary>
    /// Handles the selection changed event for a specific game.
    /// </summary>
    /// <param name="sender">The specific game whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void SpecificGame_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateBaseSpecificGames();
    }

    /// <summary>
    /// Finds games in the configured scan locations and populates the specific games collection.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task FindGames()
    {
        List<string> scanLocations = [];

        if (!string.IsNullOrWhiteSpace(GamesPath) && IsScanGames)
        {
            scanLocations.Add(GamesPath);
        }

        if (!string.IsNullOrWhiteSpace(UnfoundMediaMovePath) && IsScanUnfoundGames)
        {
            scanLocations.Add(UnfoundMediaMovePath);
        }

        SpecificGames.Clear();

        foreach (string scanLocation in scanLocations)
        {
            var folders = await FileService.GetSubFolders(scanLocation);

            foreach (var folder in folders)
            {
                foreach (var folderConfiguration in FolderConfigurations.Where(f => f.Name == FileService.GetFolderName(folder)))
                {
                    foreach (var platformConfiguration in PlatformConfigurations.Where(p => p.Code == folderConfiguration.Platform))
                    {
                        foreach (var extensionConfiguraion in ExtensionConfigurations.Where(e => e.Platform == folderConfiguration.Platform))
                        {
                            foreach (var exension in extensionConfiguraion.Extensions)
                            {
                                foreach (var file in await FileService.GetFiles(folder, $"*{exension}"))
                                {
                                    var game = new SpecificGameViewModel(new SpecificGame())
                                    {
                                        Platform = platformConfiguration,

                                        Name = FileService.GetFileNameWithoutExtension(file),
                                        Path = file
                                    };

                                    SpecificGames.Add(game);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates the specific games collection by finding games in configured locations.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateSpecificGames()
    {
        await FindGames();
    }

    /// <summary>
    /// Updates the specific platforms collection from platform configurations.
    /// </summary>
    private void UpdateSpecificPlatforms()
    {
        foreach (var platform in PlatformConfigurations)
        {
            if (!SpecificPlatforms.Any(p => p.PlatformConfiguration.Equals(platform)))
            {
                SpecificPlatforms.Add(new SpecificPlatformViewModel()
                {
                    PlatformConfiguration = platform,
                    IsSelected = base.SpecificPlatforms.Any(p => p == platform.Code)
                });
            }
        }
    }

    /// <summary>
    /// Updates the selected specific platforms collection based on selection state.
    /// </summary>
    private void UpdateSelectedSpecificPlatforms()
    {
        var slectedPlatforms = SpecificPlatforms.Where(p => p.IsSelected);

        SelectedSpecificPlatforms.Clear();

        foreach (var platform in slectedPlatforms)
        {
            SelectedSpecificPlatforms.Add(platform);
        }
    }

    /// <summary>
    /// Updates the base model's specific platforms from the view model collection.
    /// </summary>
    private void UpdateBaseSpecificPlatforms()
    {
        if (SpecificPlatforms.Any(p => !p.IsSelected))
        {
            base.SpecificPlatforms = [.. SpecificPlatforms.Where(p => p.IsSelected).Select(p => p.PlatformConfiguration.Code)];
        }
        else
        {
            base.SpecificPlatforms = [];
        }
    }

    /// <summary>
    /// Updates the base model's specific games from the view model collection.
    /// </summary>
    private void UpdateBaseSpecificGames()
    {
        if (SpecificGames.Any(g => !g.IsSelected))
        {
            base.SpecificGames = [.. SpecificGames.Where(g => g.IsSelected).Select(g => g.BaseModel)];
        }
        else
        {
            base.SpecificGames = [];
        }
    }

    /// <summary>
    /// Updates icon paths for specific platforms from platform icon configurations.
    /// </summary>
    private void UpdateSpecificPlatformsIconPaths()
    {
        foreach (var iconConfiguration in PlatformIconConfigurations)
        {
            foreach (var platfrom in SpecificPlatforms.Where(p => p.PlatformConfiguration.Code == iconConfiguration.Platform))
            {
                platfrom.IconPath = iconConfiguration.Path;
            }

            foreach (var platfrom in PlatformConfigurations.Where(p => p.Code == iconConfiguration.Platform))
            {
                platfrom.IconPath = iconConfiguration.Path;
            }
        }
    }

    /// <summary>
    /// Called when a property value changes to handle dependent updates.
    /// </summary>
    /// <param name="e">The property changed event args.</param>
    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(ApplyAssetPath):
                ApplyAssetPath = await FileService.CheckPath(ApplyAssetPath);
                break;
            case nameof(GamesPath):
                GamesPath = await FileService.CheckPath(GamesPath);
                await UpdateSpecificGames();
                break;
            case nameof(UnfoundMediaMovePath):
                UnfoundMediaMovePath = await FileService.CheckPath(UnfoundMediaMovePath);
                await UpdateSpecificGames();
                break;
            case nameof(IsScanGames):
            case nameof(IsScanUnfoundGames):
                await UpdateSpecificGames();
                break;
        }
    }

    /// <summary>
    /// Initializes event handlers for a folder name configuration.
    /// </summary>
    /// <param name="item">The folder name configuration to initialize.</param>
    protected void InitializeFolderNameConfiguration(FolderNameConfigurationViewModel item)
    {
        item.RemoveRequested += OnFolderNameConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from a folder name configuration.
    /// </summary>
    /// <param name="item">The folder name configuration to de-initialize.</param>
    protected void DeInitializeFolderNameConfiguration(FolderNameConfigurationViewModel item)
    {
        item.RemoveRequested -= OnFolderNameConfigurationRemoveRequested;
    }

    /// <summary>
    /// Initializes event handlers for a game icon overlay configuration.
    /// </summary>
    /// <param name="item">The game icon overlay configuration to initialize.</param>
    protected void InitializeGameIconOverlayConfiguration(GameIconOverlayConfigurationViewModel item)
    {
        item.RemoveRequested += OnGameIconOverlayConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from a game icon overlay configuration.
    /// </summary>
    /// <param name="item">The game icon overlay configuration to de-initialize.</param>
    protected void DeInitializeGameIconOverlayConfiguration(GameIconOverlayConfigurationViewModel item)
    {
        item.RemoveRequested -= OnGameIconOverlayConfigurationRemoveRequested;
    }

    /// <summary>
    /// Initializes event handlers for a platform icon configuration.
    /// </summary>
    /// <param name="item">The platform icon configuration to initialize.</param>
    protected void InitializePlatformIconConfiguration(PlatformIconConfigurationViewModel item)
    {
        item.RemoveRequested += OnPlatformIconConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from a platform icon configuration.
    /// </summary>
    /// <param name="item">The platform icon configuration to de-initialize.</param>
    protected void DeInitializePlatformIconConfiguration(PlatformIconConfigurationViewModel item)
    {
        item.RemoveRequested -= OnPlatformIconConfigurationRemoveRequested;
    }

    /// <summary>
    /// Initializes event handlers for an extension configuration.
    /// </summary>
    /// <param name="item">The extension configuration to initialize.</param>
    protected void InitializeExtensionConfiguration(ExtensionConfigurationViewModel item)
    {
        item.RemoveRequested += OnExtensionConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from an extension configuration.
    /// </summary>
    /// <param name="item">The extension configuration to de-initialize.</param>
    protected void DeInitializeExtensionConfiguration(ExtensionConfigurationViewModel item)
    {
        item.RemoveRequested -= OnExtensionConfigurationRemoveRequested;
    }

    /// <summary>
    /// Initializes event handlers for a scraper configuration.
    /// </summary>
    /// <param name="item">The scraper configuration to initialize.</param>
    protected void InitializeScraperConfiguration(ScraperConfigurationViewModel item)
    {
        item.RemoveRequested += OnScraperConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from a scraper configuration.
    /// </summary>
    /// <param name="item">The scraper configuration to de-initialize.</param>
    protected void DeInitializeScraperConfiguration(ScraperConfigurationViewModel item)
    {
        item.RemoveRequested -= OnScraperConfigurationRemoveRequested;
    }

    /// <summary>
    /// Initializes event handlers for an upscaler configuration.
    /// </summary>
    /// <param name="item">The upscaler configuration to initialize.</param>
    protected void InitializeUpscalerConfiguration(UpscalerConfigurationViewModel item)
    {
        item.RemoveRequested += OnUpscalerConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from an upscaler configuration.
    /// </summary>
    /// <param name="item">The upscaler configuration to de-initialize.</param>
    protected void DeInitializeUpscalerConfiguration(UpscalerConfigurationViewModel item)
    {
        item.RemoveRequested -= OnUpscalerConfigurationRemoveRequested;
    }

    /// <summary>
    /// Initializes event handlers for a platform configuration.
    /// </summary>
    /// <param name="item">The platform configuration to initialize.</param>
    protected void InitializePlatformConfiguration(PlatformConfigurationViewModel item)
    {
        item.RemoveRequested += OnPlatformConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from a platform configuration.
    /// </summary>
    /// <param name="item">The platform configuration to de-initialize.</param>
    protected void DeInitializePlatformConfiguration(PlatformConfigurationViewModel item)
    {
        item.RemoveRequested -= OnPlatformConfigurationRemoveRequested;
    }

    /// <summary>
    /// Initializes event handlers for a reconstructor configuration.
    /// </summary>
    /// <param name="item">The reconstructor configuration to initialize.</param>
    protected void InitializeReconstructorConfiguration(UpscalerConfigurationViewModel item)
    {
        item.RemoveRequested += OnReconstructorConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from a reconstructor configuration.
    /// </summary>
    /// <param name="item">The reconstructor configuration to de-initialize.</param>
    protected void DeInitializeReconstructorConfiguration(UpscalerConfigurationViewModel item)
    {
        item.RemoveRequested -= OnReconstructorConfigurationRemoveRequested;
    }

    /// <summary>
    /// Initializes event handlers for a specific platform.
    /// </summary>
    /// <param name="item">The specific platform to initialize.</param>
    protected void InitializeSpecificPlatform(SpecificPlatformViewModel item)
    {
        item.SelectedChanged += SpecificPlatform_SelectedChanged;
    }

    /// <summary>
    /// Removes event handlers from a specific platform.
    /// </summary>
    /// <param name="item">The specific platform to de-initialize.</param>
    protected void DeInitializeSpecificPlatform(SpecificPlatformViewModel item)
    {
        item.SelectedChanged -= SpecificPlatform_SelectedChanged;
    }

    /// <summary>
    /// Initializes event handlers for a specific game.
    /// </summary>
    /// <param name="item">The specific game to initialize.</param>
    protected void InitializeSpecificGame(SpecificGameViewModel item)
    {
        item.SelectionChanged += SpecificGame_SelectionChanged;
    }

    /// <summary>
    /// Removes event handlers from a specific game.
    /// </summary>
    /// <param name="item">The specific game to de-initialize.</param>
    protected void DeInitializeSpecificGame(SpecificGameViewModel item)
    {
        item.SelectionChanged -= SpecificGame_SelectionChanged;
    }

    /// <summary>
    /// Initializes event handlers for a path history entry.
    /// </summary>
    /// <param name="item">The path history entry to initialize.</param>
    protected void InitializePathHistory(PathHistoryViewModel item)
    {
        item.RemoveRequested += OnPathHistoryRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from a path history entry.
    /// </summary>
    /// <param name="item">The path history entry to de-initialize.</param>
    protected void DeInitializePathHistory(PathHistoryViewModel item)
    {
        item.RemoveRequested -= OnPathHistoryRemoveRequested;
    }

    /// <summary>
    /// Creates a folder name configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying folder name configuration model.</param>
    /// <returns>A new folder name configuration view model.</returns>
    public FolderNameConfigurationViewModel CreateFolderNameConfiguration(FolderNameConfiguration baseModel)
    {
        return new FolderNameConfigurationViewModel(baseModel, this);
    }

    /// <summary>
    /// Creates a game icon overlay configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying game icon overlay configuration model.</param>
    /// <returns>A new game icon overlay configuration view model.</returns>
    public GameIconOverlayConfigurationViewModel CreateGameIconOverlayConfiguration(GameIconOverlayConfiguration baseModel)
    {
        return new GameIconOverlayConfigurationViewModel(baseModel, this);
    }

    /// <summary>
    /// Creates a platform icon configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying platform icon configuration model.</param>
    /// <returns>A new platform icon configuration view model.</returns>
    public PlatformIconConfigurationViewModel CreatePlatformIconConfiguration(PlatformIconConfiguration baseModel)
    {
        return new PlatformIconConfigurationViewModel(baseModel, this);
    }

    /// <summary>
    /// Creates an extension configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying extension configuration model.</param>
    /// <returns>A new extension configuration view model.</returns>
    public ExtensionConfigurationViewModel CreateExtensionConfiguration(ExtensionConfiguration baseModel)
    {
        return new ExtensionConfigurationViewModel(baseModel, this);
    }

    /// <summary>
    /// Creates a scraper configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying scraper configuration model.</param>
    /// <returns>A new scraper configuration view model.</returns>
    public ScraperConfigurationViewModel CreateScraperConfiguration(ScraperConfiguration baseModel)
    {
        return new ScraperConfigurationViewModel(baseModel, this);
    }

    /// <summary>
    /// Creates an upscaler configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying upscaler configuration model.</param>
    /// <returns>A new upscaler configuration view model.</returns>
    public UpscalerConfigurationViewModel CreateUpscalerConfiguration(UpscalerConfiguration baseModel)
    {
        return new UpscalerConfigurationViewModel(baseModel);
    }

    /// <summary>
    /// Creates a platform configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying platform configuration model.</param>
    /// <returns>A new platform configuration view model.</returns>
    public PlatformConfigurationViewModel CreatePlatformConfiguration(PlatformConfiguration baseModel)
    {
        return new PlatformConfigurationViewModel(baseModel);
    }

    /// <summary>
    /// Creates a reconstructor configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying upscaler configuration model.</param>
    /// <returns>A new reconstructor configuration view model.</returns>
    public UpscalerConfigurationViewModel CreateReconstructorConfiguration(UpscalerConfiguration baseModel)
    {
        return new UpscalerConfigurationViewModel(baseModel);
    }

    /// <summary>
    /// Creates a new specific platform view model.
    /// </summary>
    /// <returns>A new specific platform view model.</returns>
    public SpecificPlatformViewModel CreateSpecificPlatform()
    {
        return new SpecificPlatformViewModel();
    }

    /// <summary>
    /// Creates a specific game view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying specific game model.</param>
    /// <returns>A new specific game view model.</returns>
    public SpecificGameViewModel CreateSpecificGame(SpecificGame baseModel)
    {
        return new SpecificGameViewModel(baseModel);
    }

    /// <summary>
    /// Creates a new folder name configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewFolderNameConfiguration()
    {
        InsertFolderNameConfiguration(0, CreateFolderNameConfiguration(new FolderNameConfiguration()));
    }

    /// <summary>
    /// Creates a new game icon overlay configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewGameIconOverlayConfiguration()
    {
        InsertGameIconOverlayConfiguration(0, CreateGameIconOverlayConfiguration(new GameIconOverlayConfiguration()));
    }

    /// <summary>
    /// Creates a new platform icon configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewPlatformIconConfiguration()
    {
        InsertPlatformIconConfiguration(0, CreatePlatformIconConfiguration(new PlatformIconConfiguration()));
    }

    /// <summary>
    /// Creates a new extension configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewExtensionConfiguration()
    {
        InsertExtensionConfiguration(0, CreateExtensionConfiguration(new ExtensionConfiguration()));
    }

    /// <summary>
    /// Creates a new scraper configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewScraperConfiguration()
    {
        InsertScraperConfiguration(0, CreateScraperConfiguration(new ScraperConfiguration()));
    }

    /// <summary>
    /// Creates a new upscaler configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewUpscalerConfiguration()
    {
        InsertUpscalerConfiguration(0, CreateUpscalerConfiguration(new UpscalerConfiguration()));
    }

    /// <summary>
    /// Creates a new platform configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewPlatformConfiguration()
    {
        InsertPlatformConfiguration(0, CreatePlatformConfiguration(new PlatformConfiguration()));
    }

    /// <summary>
    /// Creates a new reconstructor configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewReconstructorConfiguration()
    {
        InsertReconstructorConfiguration(0, CreateReconstructorConfiguration(new UpscalerConfiguration()));
    }

    /// <summary>
    /// Creates a new specific platform and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewSpecificPlatform()
    {
        InsertSpecificPlatform(0, CreateSpecificPlatform());
    }

    /// <summary>
    /// Creates a new specific game and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewSpecificGame()
    {
        InsertSpecificGame(0, CreateSpecificGame(new SpecificGame()));
    }

    /// <summary>
    /// Adds a folder name configuration to the collection.
    /// </summary>
    /// <param name="item">The folder name configuration to add.</param>
    public void AddFolderNameConfiguration(FolderNameConfigurationViewModel item)
    {
        InitializeFolderNameConfiguration(item);
        FolderConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts a folder name configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The folder name configuration to insert.</param>
    public void InsertFolderNameConfiguration(int index, FolderNameConfigurationViewModel item)
    {
        InitializeFolderNameConfiguration(item);
        FolderConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes a folder name configuration from the collection.
    /// </summary>
    /// <param name="item">The folder name configuration to remove.</param>
    public void RemoveFolderNameConfiguration(FolderNameConfigurationViewModel item)
    {
        DeInitializeFolderNameConfiguration(item);
        FolderConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all folder name configurations from the collection.
    /// </summary>
    public void ClearFolderNameConfigurations()
    {
        foreach (var item in FolderConfigurations.ToList())
        {
            RemoveFolderNameConfiguration(item);
        }
    }

    /// <summary>
    /// Adds a game icon overlay configuration to the collection.
    /// </summary>
    /// <param name="item">The game icon overlay configuration to add.</param>
    public void AddGameIconOverlayConfiguration(GameIconOverlayConfigurationViewModel item)
    {
        InitializeGameIconOverlayConfiguration(item);
        GameIconOverlayConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts a game icon overlay configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The game icon overlay configuration to insert.</param>
    public void InsertGameIconOverlayConfiguration(int index, GameIconOverlayConfigurationViewModel item)
    {
        InitializeGameIconOverlayConfiguration(item);
        GameIconOverlayConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes a game icon overlay configuration from the collection.
    /// </summary>
    /// <param name="item">The game icon overlay configuration to remove.</param>
    public void RemoveGameIconOverlayConfiguration(GameIconOverlayConfigurationViewModel item)
    {
        DeInitializeGameIconOverlayConfiguration(item);
        GameIconOverlayConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all game icon overlay configurations from the collection.
    /// </summary>
    public void ClearGameIconOverlayConfigurations()
    {
        foreach (var item in GameIconOverlayConfigurations.ToList())
        {
            RemoveGameIconOverlayConfiguration(item);
        }
    }

    /// <summary>
    /// Adds a platform icon configuration to the collection.
    /// </summary>
    /// <param name="item">The platform icon configuration to add.</param>
    public void AddPlatformIconConfiguration(PlatformIconConfigurationViewModel item)
    {
        InitializePlatformIconConfiguration(item);
        PlatformIconConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts a platform icon configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The platform icon configuration to insert.</param>
    public void InsertPlatformIconConfiguration(int index, PlatformIconConfigurationViewModel item)
    {
        InitializePlatformIconConfiguration(item);
        PlatformIconConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes a platform icon configuration from the collection.
    /// </summary>
    /// <param name="item">The platform icon configuration to remove.</param>
    public void RemovePlatformIconConfiguration(PlatformIconConfigurationViewModel item)
    {
        DeInitializePlatformIconConfiguration(item);
        PlatformIconConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all platform icon configurations from the collection.
    /// </summary>
    public void ClearPlatformIconConfigurations()
    {
        foreach (var item in PlatformIconConfigurations.ToList())
        {
            RemovePlatformIconConfiguration(item);
        }
    }

    /// <summary>
    /// Adds an extension configuration to the collection.
    /// </summary>
    /// <param name="item">The extension configuration to add.</param>
    public void AddExtensionConfiguration(ExtensionConfigurationViewModel item)
    {
        InitializeExtensionConfiguration(item);
        ExtensionConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts an extension configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The extension configuration to insert.</param>
    public void InsertExtensionConfiguration(int index, ExtensionConfigurationViewModel item)
    {
        InitializeExtensionConfiguration(item);
        ExtensionConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes an extension configuration from the collection.
    /// </summary>
    /// <param name="item">The extension configuration to remove.</param>
    public void RemoveExtensionConfiguration(ExtensionConfigurationViewModel item)
    {
        DeInitializeExtensionConfiguration(item);
        ExtensionConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all extension configurations from the collection.
    /// </summary>
    public void ClearExtensionConfigurations()
    {
        foreach (var item in ExtensionConfigurations.ToList())
        {
            RemoveExtensionConfiguration(item);
        }
    }

    /// <summary>
    /// Adds a scraper configuration to the collection.
    /// </summary>
    /// <param name="item">The scraper configuration to add.</param>
    public void AddScraperConfiguration(ScraperConfigurationViewModel item)
    {
        InitializeScraperConfiguration(item);
        ScraperConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts a scraper configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The scraper configuration to insert.</param>
    public void InsertScraperConfiguration(int index, ScraperConfigurationViewModel item)
    {
        InitializeScraperConfiguration(item);
        ScraperConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes a scraper configuration from the collection.
    /// </summary>
    /// <param name="item">The scraper configuration to remove.</param>
    public void RemoveScraperConfiguration(ScraperConfigurationViewModel item)
    {
        DeInitializeScraperConfiguration(item);
        ScraperConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all scraper configurations from the collection.
    /// </summary>
    public void ClearScraperConfigurations()
    {
        foreach (var item in ScraperConfigurations.ToList())
        {
            RemoveScraperConfiguration(item);
        }
    }

    /// <summary>
    /// Adds an upscaler configuration to the collection.
    /// </summary>
    /// <param name="item">The upscaler configuration to add.</param>
    public void AddUpscalerConfiguration(UpscalerConfigurationViewModel item)
    {
        InitializeUpscalerConfiguration(item);
        UpscalerConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts an upscaler configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The upscaler configuration to insert.</param>
    public void InsertUpscalerConfiguration(int index, UpscalerConfigurationViewModel item)
    {
        InitializeUpscalerConfiguration(item);
        UpscalerConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes an upscaler configuration from the collection.
    /// </summary>
    /// <param name="item">The upscaler configuration to remove.</param>
    public void RemoveUpscalerConfiguration(UpscalerConfigurationViewModel item)
    {
        DeInitializeUpscalerConfiguration(item);
        UpscalerConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all upscaler configurations from the collection.
    /// </summary>
    public void ClearUpscalerConfigurations()
    {
        foreach (var item in UpscalerConfigurations.ToList())
        {
            RemoveUpscalerConfiguration(item);
        }
    }

    /// <summary>
    /// Adds a platform configuration to the collection.
    /// </summary>
    /// <param name="item">The platform configuration to add.</param>
    public void AddPlatformConfiguration(PlatformConfigurationViewModel item)
    {
        InitializePlatformConfiguration(item);
        PlatformConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts a platform configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The platform configuration to insert.</param>
    public void InsertPlatformConfiguration(int index, PlatformConfigurationViewModel item)
    {
        InitializePlatformConfiguration(item);
        PlatformConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes a platform configuration from the collection.
    /// </summary>
    /// <param name="item">The platform configuration to remove.</param>
    public void RemovePlatformConfiguration(PlatformConfigurationViewModel item)
    {
        DeInitializePlatformConfiguration(item);
        PlatformConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all platform configurations from the collection.
    /// </summary>
    public void ClearPlatformConfigurations()
    {
        foreach (var item in PlatformConfigurations.ToList())
        {
            RemovePlatformConfiguration(item);
        }
    }

    /// <summary>
    /// Adds a reconstructor configuration to the collection.
    /// </summary>
    /// <param name="item">The reconstructor configuration to add.</param>
    public void AddReconstructorConfiguration(UpscalerConfigurationViewModel item)
    {
        InitializeReconstructorConfiguration(item);
        ReconstructorConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts a reconstructor configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The reconstructor configuration to insert.</param>
    public void InsertReconstructorConfiguration(int index, UpscalerConfigurationViewModel item)
    {
        InitializeReconstructorConfiguration(item);
        ReconstructorConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes a reconstructor configuration from the collection.
    /// </summary>
    /// <param name="item">The reconstructor configuration to remove.</param>
    public void RemoveReconstructorConfiguration(UpscalerConfigurationViewModel item)
    {
        DeInitializeReconstructorConfiguration(item);
        ReconstructorConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all reconstructor configurations from the collection.
    /// </summary>
    public void ClearReconstructorConfigurations()
    {
        foreach (var item in ReconstructorConfigurations.ToList())
        {
            RemoveReconstructorConfiguration(item);
        }
    }

    /// <summary>
    /// Adds a specific platform to the collection.
    /// </summary>
    /// <param name="item">The specific platform to add.</param>
    public void AddSpecificPlatform(SpecificPlatformViewModel item)
    {
        SpecificPlatforms.Add(item);
    }

    /// <summary>
    /// Inserts a specific platform at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The specific platform to insert.</param>
    public void InsertSpecificPlatform(int index, SpecificPlatformViewModel item)
    {
        SpecificPlatforms.Insert(index, item);
    }

    /// <summary>
    /// Removes a specific platform from the collection.
    /// </summary>
    /// <param name="item">The specific platform to remove.</param>
    public void RemoveSpecificPlatform(SpecificPlatformViewModel item)
    {
        SpecificPlatforms.Remove(item);
    }

    /// <summary>
    /// Clears all specific platforms from the collection.
    /// </summary>
    public void ClearSpecificPlatforms()
    {
        foreach (var item in SpecificPlatforms.ToList())
        {
            RemoveSpecificPlatform(item);
        }
    }

    /// <summary>
    /// Adds a specific game to the collection.
    /// </summary>
    /// <param name="item">The specific game to add.</param>
    public void AddSpecificGame(SpecificGameViewModel item)
    {
        InitializeSpecificGame(item);
        SpecificGames.Add(item);
    }

    /// <summary>
    /// Inserts a specific game at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The specific game to insert.</param>
    public void InsertSpecificGame(int index, SpecificGameViewModel item)
    {
        InitializeSpecificGame(item);
        SpecificGames.Insert(index, item);
    }

    /// <summary>
    /// Removes a specific game from the collection.
    /// </summary>
    /// <param name="item">The specific game to remove.</param>
    public void RemoveSpecificGame(SpecificGameViewModel item)
    {
        DeInitializeSpecificGame(item);
        SpecificGames.Remove(item);
    }

    /// <summary>
    /// Clears all specific games from the collection.
    /// </summary>
    public void ClearSpecificGames()
    {
        foreach (var item in SpecificGames.ToList())
        {
            RemoveSpecificGame(item);
        }
    }

    /// <summary>
    /// Returns games path history entries matching the query text.
    /// </summary>
    /// <param name="query">The search text to filter by.</param>
    /// <returns>Matching path history entries.</returns>
    public IEnumerable<PathHistoryViewModel> FindGamesPath(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GamesPathHistory;

        return GamesPathHistory.Where(p => p.Path.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sets the games path from a chosen history entry.
    /// </summary>
    /// <param name="path">The path to set.</param>
    public void ChooseGamesPath(string path)
    {
        GamesPath = path;
    }

    /// <summary>
    /// Returns apply asset path history entries matching the query text.
    /// </summary>
    /// <param name="query">The search text to filter by.</param>
    /// <returns>Matching path history entries.</returns>
    public IEnumerable<PathHistoryViewModel> FindApplyAssetPath(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ApplyAssetPathHistory;

        return ApplyAssetPathHistory.Where(p => p.Path.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sets the apply asset path from a chosen history entry.
    /// </summary>
    /// <param name="path">The path to set.</param>
    public void ChooseApplyAssetPath(string path)
    {
        ApplyAssetPath = path;
    }

    /// <summary>
    /// Returns unfound media move path history entries matching the query text.
    /// </summary>
    /// <param name="query">The search text to filter by.</param>
    /// <returns>Matching path history entries.</returns>
    public IEnumerable<PathHistoryViewModel> FindUnfoundMediaMovePath(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return UnfoundMediaMovePathHistory;

        return UnfoundMediaMovePathHistory.Where(p => p.Path.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sets the unfound media move path from a chosen history entry.
    /// </summary>
    /// <param name="path">The path to set.</param>
    public void ChooseUnfoundMediaMovePath(string path)
    {
        UnfoundMediaMovePath = path;
    }

    /// <summary>
    /// Adds the current path values to their respective history lists if not already present.
    /// </summary>
    public void AddCurrentPathsToHistory()
    {
        AddToHistory(GamesPathHistory, GamesPath);
        AddToHistory(ApplyAssetPathHistory, ApplyAssetPath);
        AddToHistory(UnfoundMediaMovePathHistory, UnfoundMediaMovePath);
    }

    /// <summary>
    /// Adds the path value to its respective history lists if not already present.
    /// </summary>
    private void AddToHistory(ObservableCollection<PathHistoryViewModel> history, string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) && !history.Any(h => h.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
        {
            var item = new PathHistoryViewModel(path);

            InitializePathHistory(item);

            history.Add(item);
        }
    }

    /// <summary>
    /// Requests removal of the configuration.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Selects all specific platforms.
    /// </summary>
    [RelayCommand]
    public void SelectAllSpecificPlatforms()
    {
        foreach (var platform in SpecificPlatforms)
        {
            platform.IsSelected = true;
        }

        BaseModel.SpecificPlatforms.Clear();
    }

    /// <summary>
    /// Clears all specific platform selections.
    /// </summary>
    [RelayCommand]
    public void ClearAllSpecificPlatformSelections()
    {
        foreach (var platform in SpecificPlatforms)
        {
            platform.IsSelected = false;
        }
    }

    /// <summary>
    /// Selects all specific games.
    /// </summary>
    [RelayCommand]
    public void SelectAllSpecificGames()
    {
        foreach (var game in SpecificGames)
        {
            game.IsSelected = true;
        }

        BaseModel.SpecificGames.Clear();
    }

    /// <summary>
    /// Clears all specific game selections.
    /// </summary>
    [RelayCommand]
    public void ClearAllSpecificGameSelections()
    {
        foreach (var game in SpecificGames)
        {
            game.IsSelected = false;
        }
    }

    /// <summary>
    /// Requests to show the configuration dialog.
    /// </summary>
    [RelayCommand]
    public void RequestShowConfiguraiton()
    {
        ShowConfiguraitonRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the file service.
    /// </summary>
    protected IFileService FileService { get; private set; }
}
