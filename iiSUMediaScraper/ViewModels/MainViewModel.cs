using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.ViewModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the main view model for the application.
/// </summary>
public partial class MainViewModel : ObservableRecipient
{
    /// <summary>
    /// The maximum number of concurrent operations allowed.
    /// </summary>
    public const int MaxConcurrency = 100;

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is being loaded.
    /// </summary>
    [ObservableProperty]
    private bool isLoadingConfiguration;

    /// <summary>
    /// Gets or sets a value indicating whether the scraping process has begun.
    /// </summary>
    [ObservableProperty]
    private bool hasBegun;

    /// <summary>
    /// Gets or sets a value indicating whether the configuration dialog is shown.
    /// </summary>
    [ObservableProperty]
    private bool isShowingConfiguration;

    /// <summary>
    /// Gets or sets a value indicating whether the apply configuration dialog is shown.
    /// </summary>
    [ObservableProperty]
    private bool isShowingApplyConfiguration;

    /// <summary>
    /// Gets or sets a value indicating whether media is being applied.
    /// </summary>
    [ObservableProperty]
    private bool isApplying;

    /// <summary>
    /// Gets or sets a value indicating whether applying has finished.
    /// </summary>
    [ObservableProperty]
    private bool isApplyingFinished;

    /// <summary>
    /// Gets or sets a value indicating whether data is being loaded.
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// Gets or sets the count of applied games.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ApplyingProgress))]
    private int appliedGamesCount;

    /// <summary>
    /// Gets or sets the currently applying game.
    /// </summary>
    [ObservableProperty]
    private GameViewModel? applyingGame;

    /// <summary>
    /// Gets or sets the selected platform.
    /// </summary>
    [ObservableProperty]
    private PlatformViewModel? selectedPlatform;

    /// <summary>
    /// Gets or sets the configuration view model.
    /// </summary>
    [ObservableProperty]
    private ConfigurationViewModel? configuration;

    /// <summary>
    /// Gets or sets the collection of platforms.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PlatformViewModel> platforms;

    /// <summary>
    /// Gets or sets the collection of games to be applied.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<GameViewModel> applyingGames;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="fileService">The file service.</param>
    /// <param name="scrapingService">The scraping service.</param>
    /// <param name="imageFormatterService">The image formatter service.</param>
    /// <param name="upscalerService">The upscaler service.</param>
    public MainViewModel(IConfigurationService configurationService, IFileService fileService, IScrapingService scrapingService, IImageFormatterService imageFormatterService, IUpscalerService upscalerService)
    {
        ConfigurationService = configurationService;

        FileService = fileService;

        ScrapingService = scrapingService;

        ImageFormatterService = imageFormatterService;

        UpscalerService = upscalerService;

        isShowingConfiguration = true;

        platforms = [];

        applyingGames = [];

        Load();
    }

    private void Configuration_ShowConfiguraitonRequested(object? sender, EventArgs e)
    {
        _ = ShowConfiguration();
    }

    private async void StageApplyGamesRequested(object? sender, IEnumerable<GameViewModel> e)
    {
        await StageApplyGames(e);
    }

    private void InitializePlatform(PlatformViewModel platform)
    {
        platform.StageApplyGamesRequested += StageApplyGamesRequested;
    }

    private void DeInitializePlatform(PlatformViewModel platform)
    {
        platform.StageApplyGamesRequested -= StageApplyGamesRequested;
    }

    protected async Task StageApplyGames(IEnumerable<GameViewModel> games)
    {
        IsShowingApplyConfiguration = true;

        ApplyingGames.Clear();

        foreach (GameViewModel game in games)
        {
            ApplyingGames.Add(game);
        }

        OnPropertyChanged(nameof(ApplyingPlatforms));
    }

    /// <summary>
    /// Shows the configuration dialog.
    /// </summary>
    [RelayCommand]
    public async Task ShowConfiguration()
    {
        IsShowingConfiguration = true;
    }

    /// <summary>
    /// Hides the configuration dialog.
    /// </summary>
    [RelayCommand]
    public async Task HideConfiguration()
    {
        IsShowingConfiguration = false;
    }

    /// <summary>
    /// Saves the current configuration.
    /// </summary>
    [RelayCommand]
    public async Task SaveConfiguration()
    {
        ConfigurationService?.SaveConfiguration();
    }

    /// <summary>
    /// Stages all games for applying.
    /// </summary>
    [RelayCommand]
    public async Task StageApply()
    {
        await StageApplyGames(Platforms.Select(p => p.Games).SelectMany(g => g));
    }

    /// <summary>
    /// Hides the apply configuration dialog.
    /// </summary>
    [RelayCommand]
    public async Task HideApplyConfiguration()
    {
        IsShowingApplyConfiguration = false;

        IsApplyingFinished = false;
    }

    /// <summary>
    /// Applies media assets to all staged games.
    /// </summary>
    [RelayCommand]
    public async Task Apply()
    {
        IsApplying = true;

        await SaveConfiguration();

        AppliedGamesCount = 0;

        foreach (GameViewModel game in ApplyingGames)
        {
            ApplyingGame = game;

            await game.Apply();

            AppliedGamesCount++;
        }

        IsApplying = false;

        IsApplyingFinished = true;
    }

    /// <summary>
    /// Begins the scraping process for all configured games.
    /// </summary>
    [RelayCommand]
    public async Task Begin()
    {
        HasBegun = true;

        foreach (PlatformViewModel? platform in Platforms.ToList())
        {
            DeInitializePlatform(platform);

            Platforms.Remove(platform);
        }

        SelectedPlatform = null;

        await HideConfiguration();

        await SaveConfiguration();

        if (Configuration != null)
        {
            List<string> scanLocations = [];

            if (!string.IsNullOrWhiteSpace(Configuration.GamesPath) && Configuration.IsScanGames)
            {
                scanLocations.Add(Configuration.GamesPath);
            }

            if (!string.IsNullOrWhiteSpace(Configuration.UnfoundMediaMovePath) && Configuration.IsScanUnfoundGames)
            {
                scanLocations.Add(Configuration.UnfoundMediaMovePath);
            }

            IsLoading = true;

            SemaphoreSlim semaphore = new SemaphoreSlim((int)Math.Min(MaxConcurrency, Configuration.MaxNumberOfConcurrentGames));

            foreach (string scanLocation in scanLocations)
            {
                IEnumerable<string> folders = await FileService.GetSubFolders(scanLocation);

                foreach (string folder in folders)
                {
                    foreach (FolderNameConfigurationViewModel? folderConfiguration in Configuration.FolderConfigurations.Where(f => f.Name == FileService.GetFolderName(folder)))
                    {
                        foreach (PlatformConfigurationViewModel? platformConfiguration in Configuration.PlatformConfigurations.Where(p => p.Code == folderConfiguration.Platform))
                        {
                            PlatformViewModel? platform = Platforms.FirstOrDefault(p => p.PlatformConfiguration.Code == platformConfiguration.Code);

                            if (platform != null)
                            {
                                platform.Folders.Add(folder);
                            }
                            else
                            {
                                platform = new PlatformViewModel(FileService, ScrapingService, ImageFormatterService, UpscalerService, Configuration, platformConfiguration, semaphore);

                                InitializePlatform(platform);

                                platform.Folders.Add(folder);

                                if (Configuration.SpecificPlatforms.Any(p => p.IsSelected))
                                {
                                    if (Configuration.SpecificPlatforms.Any(p => p.IsSelected && p.PlatformConfiguration.Code == platform.PlatformConfiguration.Code))
                                    {
                                        Platforms.Add(platform);
                                    }
                                }
                                else
                                {
                                    Platforms.Add(platform);
                                }
                            }
                        }
                    }
                }
            }

            SelectedPlatform = Platforms.FirstOrDefault();

            foreach (PlatformViewModel platform in Platforms)
            {
                platform.IsLoading = true;

                await platform.FindGames();

                foreach (GameViewModel game in platform.Games)
                {
                    game.IsLoading = true;
                }
            }

            foreach (PlatformViewModel platform in Platforms)
            {
                await platform.Scrape();
            }

            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads the configuration from the configuration service.
    /// </summary>
    [RelayCommand]
    public async Task Load()
    {
        IsLoadingConfiguration = true;

        await ConfigurationService.LoadConfiguration();

        if (ConfigurationService.Configuration != null)
        {
            if (Configuration != null)
            {
                Configuration.ShowConfiguraitonRequested -= Configuration_ShowConfiguraitonRequested;
            }

            Configuration = new ConfigurationViewModel(ConfigurationService.Configuration, FileService);

            Configuration.ShowConfiguraitonRequested += Configuration_ShowConfiguraitonRequested;
        }

        IsLoadingConfiguration = false;
    }

    /// <summary>
    /// Gets the configuration service.
    /// </summary>
    protected IConfigurationService ConfigurationService { get; private set; }

    /// <summary>
    /// Gets the file service.
    /// </summary>
    protected IFileService FileService { get; private set; }

    /// <summary>
    /// Gets the scraping service.
    /// </summary>
    protected IScrapingService ScrapingService { get; private set; }

    /// <summary>
    /// Gets the image formatter service.
    /// </summary>
    protected IImageFormatterService ImageFormatterService { get; private set; }

    /// <summary>
    /// Gets the upscaler service.
    /// </summary>
    protected IUpscalerService UpscalerService { get; private set; }

    /// <summary>
    /// Gets the platforms that have games being applied.
    /// </summary>
    public IEnumerable<PlatformViewModel> ApplyingPlatforms => Platforms.Where(p => ApplyingGames.Select(g => g.Platform).Contains(p.PlatformConfiguration.Code));

    /// <summary>
    /// Gets the progress percentage of applying games.
    /// </summary>
    public int ApplyingProgress => ApplyingGames.Count > 0 ? (int)((AppliedGamesCount / (double)ApplyingGames.Count) * 100) : 0;
}
