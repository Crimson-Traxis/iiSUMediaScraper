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
    /// Gets or sets the maximum number of concurrent games to process.
    /// </summary>
    [ObservableProperty]
    private double maxNumberOfConcurrentGames = double.NaN;

    [ObservableProperty]
    private string specificGamesSearch;

    [ObservableProperty]
    private string platformDefinitionConfigurationsSearch;

    [ObservableProperty]
    private string specificPlatformsSearch;

    [ObservableProperty]
    private string imageUpscalerConfigurationsSearch;

    [ObservableProperty]
    private string imageReconstructorConfigurationsSearch;

    [ObservableProperty]
    private string folderConfigurationsSearch;

    [ObservableProperty]
    private string gameIconOverlayConfigurationsSearch;

    [ObservableProperty]
    private string platformIconConfigurationsSearch;

    [ObservableProperty]
    private string extensionConfigurationsSearch;

    [ObservableProperty]
    private UpscalerConfigurationViewModel? selectedUpscalerConfiguration;

    [ObservableProperty]
    private UpscalerConfigurationViewModel? selectedReconstructorConfiguration;

    [ObservableProperty]
    private ObservableCollection<FolderNameConfigurationViewModel> folderConfigurations;

    [ObservableProperty]
    private ObservableCollection<GameIconOverlayConfigurationViewModel> gameIconOverlayConfigurations;

    [ObservableProperty]
    public ObservableCollection<PlatformIconConfigurationViewModel> platformIconConfigurations;

    [ObservableProperty]
    private ObservableCollection<ExtensionConfigurationViewModel> extensionConfigurations;

    [ObservableProperty]
    private ObservableCollection<ScraperConfigurationViewModel> scraperConfigurations;

    [ObservableProperty]
    private ObservableCollection<UpscalerConfigurationViewModel> upscalerConfigurations;

    [ObservableProperty]
    private ObservableCollection<PlatformConfigurationViewModel> platformConfigurations;

    [ObservableProperty]
    private ObservableCollection<UpscalerConfigurationViewModel> reconstructorConfigurations;

    [ObservableProperty]
    private ObservableCollection<SpecificPlatformViewModel> specificPlatforms;

    [ObservableProperty]
    private ObservableCollection<SpecificGameViewModel> specificGames;

    [ObservableProperty]
    private ObservableCollection<SpecificPlatformViewModel> selectedSpecificPlatforms;

    public event EventHandler? ShowConfiguraitonRequested;

    public event EventHandler? SpecificGamesSearchChanged;

    public event EventHandler? PlatformDefinitionConfigurationsSearchChanged;

    public event EventHandler? SpecificPlatformsSearchChanged;

    public event EventHandler? ImageUpscalerConfigurationsSearchChanged;

    public event EventHandler? ImageReconstructorConfigurationsSearchChanged;

    public event EventHandler? FolderConfigurationsSearchChanged;

    public event EventHandler? GameIconOverlayConfigurationsSearchChanged;

    public event EventHandler? PlatformIconConfigurationsSearchChanged;

    public event EventHandler? ExtensionConfigurationsSearchChanged;

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

        selectedSpecificPlatforms = [];

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

        selectedUpscalerConfiguration ??= upscalerConfigurations.FirstOrDefault();

        selectedReconstructorConfiguration ??= reconstructorConfigurations.FirstOrDefault();

        specificPlatforms.CollectionChanged += delegate { UpdateSpecificPlatforms(); };

        UpdateSpecificPlatforms();

        platformIconConfigurations.CollectionChanged += delegate { UpdateSpecificPlatformsIconPaths(); };

        UpdateSpecificPlatformsIconPaths();

        UpdateSpecificGames();

        UpdateSelectedSpecificPlatforms();
    }

    partial void OnSpecificGamesSearchChanged(string value)
    {
        SpecificGamesSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnPlatformDefinitionConfigurationsSearchChanged(string value)
    {
        PlatformDefinitionConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnSpecificPlatformsSearchChanged(string value)
    {
        SpecificPlatformsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnImageUpscalerConfigurationsSearchChanged(string value)
    {
        ImageUpscalerConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnImageReconstructorConfigurationsSearchChanged(string value)
    {
        ImageReconstructorConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnFolderConfigurationsSearchChanged(string value)
    {
        FolderConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnGameIconOverlayConfigurationsSearchChanged(string value)
    {
        GameIconOverlayConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnPlatformIconConfigurationsSearchChanged(string value)
    {
        PlatformIconConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnExtensionConfigurationsSearchChanged(string value)
    {
        ExtensionConfigurationsSearchChanged?.Invoke(this, EventArgs.Empty);
    }

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

    partial void OnSelectedUpscalerConfigurationChanged(UpscalerConfigurationViewModel? value)
    {
        base.DefaultUpscaleConfigurationName = value?.Name;
    }

    partial void OnSelectedReconstructorConfigurationChanged(UpscalerConfigurationViewModel? value)
    {
        base.DefaultReconstructorConfigurationName = value?.Name;
    }

    private void OnFolderNameConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is FolderNameConfigurationViewModel item)
        {
            RemoveFolderNameConfiguration(item);
        }
    }

    private void OnGameIconOverlayConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is GameIconOverlayConfigurationViewModel item)
        {
            RemoveGameIconOverlayConfiguration(item);
        }
    }

    private void OnPlatformIconConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is PlatformIconConfigurationViewModel item)
        {
            RemovePlatformIconConfiguration(item);
        }
    }

    private void OnExtensionConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ExtensionConfigurationViewModel item)
        {
            RemoveExtensionConfiguration(item);
        }
    }

    private void OnScraperConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ScraperConfigurationViewModel item)
        {
            RemoveScraperConfiguration(item);
        }
    }

    private void OnUpscalerConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is UpscalerConfigurationViewModel item)
        {
            RemoveUpscalerConfiguration(item);
        }
    }

    private void OnPlatformConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is PlatformConfigurationViewModel item)
        {
            RemovePlatformConfiguration(item);
        }
    }

    private void OnReconstructorConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is UpscalerConfigurationViewModel item)
        {
            RemoveReconstructorConfiguration(item);
        }
    }

    private void OnSpecificGameRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is SpecificGameViewModel item)
        {
            RemoveSpecificGame(item);
        }
    }

    private void SpecificPlatform_SelectedChanged(object? sender, EventArgs e)
    {
        UpdateBaseSpecificPlatforms();

        UpdateSelectedSpecificPlatforms();
    }

    private void SpecificGame_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateBaseSpecificGames();
    }

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
                                    SpecificGameViewModel game = new SpecificGameViewModel(new SpecificGame())
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

    private async Task UpdateSpecificGames()
    {
        await FindGames();
    }

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

    private void UpdateSelectedSpecificPlatforms()
    {
        var slectedPlatforms = SpecificPlatforms.Where(p => p.IsSelected);

        SelectedSpecificPlatforms.Clear();

        foreach (var platform in slectedPlatforms)
        {
            SelectedSpecificPlatforms.Add(platform);
        }
    }

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

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(GamesPath):
            case nameof(UnfoundMediaMovePath):
            case nameof(IsScanGames):
            case nameof(IsScanUnfoundGames):
                UpdateSpecificGames();
                break;
        }

        base.OnPropertyChanged(e);
    }

    protected void InitializeFolderNameConfiguration(FolderNameConfigurationViewModel item)
    {
        item.RemoveRequested += OnFolderNameConfigurationRemoveRequested;
    }

    protected void DeInitializeFolderNameConfiguration(FolderNameConfigurationViewModel item)
    {
        item.RemoveRequested -= OnFolderNameConfigurationRemoveRequested;
    }

    protected void InitializeGameIconOverlayConfiguration(GameIconOverlayConfigurationViewModel item)
    {
        item.RemoveRequested += OnGameIconOverlayConfigurationRemoveRequested;
    }

    protected void DeInitializeGameIconOverlayConfiguration(GameIconOverlayConfigurationViewModel item)
    {
        item.RemoveRequested -= OnGameIconOverlayConfigurationRemoveRequested;
    }

    protected void InitializePlatformIconConfiguration(PlatformIconConfigurationViewModel item)
    {
        item.RemoveRequested += OnPlatformIconConfigurationRemoveRequested;
    }

    protected void DeInitializePlatformIconConfiguration(PlatformIconConfigurationViewModel item)
    {
        item.RemoveRequested -= OnPlatformIconConfigurationRemoveRequested;
    }

    protected void InitializeExtensionConfiguration(ExtensionConfigurationViewModel item)
    {
        item.RemoveRequested += OnExtensionConfigurationRemoveRequested;
    }

    protected void DeInitializeExtensionConfiguration(ExtensionConfigurationViewModel item)
    {
        item.RemoveRequested -= OnExtensionConfigurationRemoveRequested;
    }

    protected void InitializeScraperConfiguration(ScraperConfigurationViewModel item)
    {
        item.RemoveRequested += OnScraperConfigurationRemoveRequested;
    }

    protected void DeInitializeScraperConfiguration(ScraperConfigurationViewModel item)
    {
        item.RemoveRequested -= OnScraperConfigurationRemoveRequested;
    }

    protected void InitializeUpscalerConfiguration(UpscalerConfigurationViewModel item)
    {
        item.RemoveRequested += OnUpscalerConfigurationRemoveRequested;
    }

    protected void DeInitializeUpscalerConfiguration(UpscalerConfigurationViewModel item)
    {
        item.RemoveRequested -= OnUpscalerConfigurationRemoveRequested;
    }

    protected void InitializePlatformConfiguration(PlatformConfigurationViewModel item)
    {
        item.RemoveRequested += OnPlatformConfigurationRemoveRequested;
    }

    protected void DeInitializePlatformConfiguration(PlatformConfigurationViewModel item)
    {
        item.RemoveRequested -= OnPlatformConfigurationRemoveRequested;
    }

    protected void InitializeReconstructorConfiguration(UpscalerConfigurationViewModel item)
    {
        item.RemoveRequested += OnReconstructorConfigurationRemoveRequested;
    }

    protected void DeInitializeReconstructorConfiguration(UpscalerConfigurationViewModel item)
    {
        item.RemoveRequested -= OnReconstructorConfigurationRemoveRequested;
    }

    protected void InitializeSpecificPlatform(SpecificPlatformViewModel item)
    {
        item.SelectedChanged += SpecificPlatform_SelectedChanged;
    }

    protected void DeInitializeSpecificPlatform(SpecificPlatformViewModel item)
    {
        item.SelectedChanged -= SpecificPlatform_SelectedChanged;
    }

    protected void InitializeSpecificGame(SpecificGameViewModel item)
    {
        item.SelectionChanged += SpecificGame_SelectionChanged;
    }

    protected void DeInitializeSpecificGame(SpecificGameViewModel item)
    {
        item.SelectionChanged -= SpecificGame_SelectionChanged;
    }

    public FolderNameConfigurationViewModel CreateFolderNameConfiguration(FolderNameConfiguration baseModel)
    {
        return new FolderNameConfigurationViewModel(baseModel, this);
    }

    public GameIconOverlayConfigurationViewModel CreateGameIconOverlayConfiguration(GameIconOverlayConfiguration baseModel)
    {
        return new GameIconOverlayConfigurationViewModel(baseModel, this);
    }

    public PlatformIconConfigurationViewModel CreatePlatformIconConfiguration(PlatformIconConfiguration baseModel)
    {
        return new PlatformIconConfigurationViewModel(baseModel, this);
    }

    public ExtensionConfigurationViewModel CreateExtensionConfiguration(ExtensionConfiguration baseModel)
    {
        return new ExtensionConfigurationViewModel(baseModel, this);
    }

    public ScraperConfigurationViewModel CreateScraperConfiguration(ScraperConfiguration baseModel)
    {
        return new ScraperConfigurationViewModel(baseModel, this);
    }

    public UpscalerConfigurationViewModel CreateUpscalerConfiguration(UpscalerConfiguration baseModel)
    {
        return new UpscalerConfigurationViewModel(baseModel);
    }

    public PlatformConfigurationViewModel CreatePlatformConfiguration(PlatformConfiguration baseModel)
    {
        return new PlatformConfigurationViewModel(baseModel);
    }

    public UpscalerConfigurationViewModel CreateReconstructorConfiguration(UpscalerConfiguration baseModel)
    {
        return new UpscalerConfigurationViewModel(baseModel);
    }

    public SpecificPlatformViewModel CreateSpecificPlatform()
    {
        return new SpecificPlatformViewModel();
    }

    public SpecificGameViewModel CreateSpecificGame(SpecificGame baseModel)
    {
        return new SpecificGameViewModel(baseModel);
    }

    [RelayCommand]
    public void CreateNewFolderNameConfiguration()
    {
        InsertFolderNameConfiguration(0, CreateFolderNameConfiguration(new FolderNameConfiguration()));
    }

    [RelayCommand]
    public void CreateNewGameIconOverlayConfiguration()
    {
        InsertGameIconOverlayConfiguration(0, CreateGameIconOverlayConfiguration(new GameIconOverlayConfiguration()));
    }

    [RelayCommand]
    public void CreateNewPlatformIconConfiguration()
    {
        InsertPlatformIconConfiguration(0, CreatePlatformIconConfiguration(new PlatformIconConfiguration()));
    }

    [RelayCommand]
    public void CreateNewExtensionConfiguration()
    {
        InsertExtensionConfiguration(0, CreateExtensionConfiguration(new ExtensionConfiguration()));
    }

    [RelayCommand]
    public void CreateNewScraperConfiguration()
    {
        InsertScraperConfiguration(0, CreateScraperConfiguration(new ScraperConfiguration()));
    }

    [RelayCommand]
    public void CreateNewUpscalerConfiguration()
    {
        InsertUpscalerConfiguration(0, CreateUpscalerConfiguration(new UpscalerConfiguration()));
    }

    [RelayCommand]
    public void CreateNewPlatformConfiguration()
    {
        InsertPlatformConfiguration(0, CreatePlatformConfiguration(new PlatformConfiguration()));
    }

    [RelayCommand]
    public void CreateNewReconstructorConfiguration()
    {
        InsertReconstructorConfiguration(0, CreateReconstructorConfiguration(new UpscalerConfiguration()));
    }

    [RelayCommand]
    public void CreateNewSpecificPlatform()
    {
        InsertSpecificPlatform(0, CreateSpecificPlatform());
    }

    [RelayCommand]
    public void CreateNewSpecificGame()
    {
        InsertSpecificGame(0, CreateSpecificGame(new SpecificGame()));
    }

    public void AddFolderNameConfiguration(FolderNameConfigurationViewModel item)
    {
        InitializeFolderNameConfiguration(item);
        FolderConfigurations.Add(item);
    }

    public void InsertFolderNameConfiguration(int index, FolderNameConfigurationViewModel item)
    {
        InitializeFolderNameConfiguration(item);
        FolderConfigurations.Insert(index, item);
    }

    public void RemoveFolderNameConfiguration(FolderNameConfigurationViewModel item)
    {
        DeInitializeFolderNameConfiguration(item);
        FolderConfigurations.Remove(item);
    }

    public void ClearFolderNameConfigurations()
    {
        foreach (var item in FolderConfigurations.ToList())
        {
            RemoveFolderNameConfiguration(item);
        }
    }

    public void AddGameIconOverlayConfiguration(GameIconOverlayConfigurationViewModel item)
    {
        InitializeGameIconOverlayConfiguration(item);
        GameIconOverlayConfigurations.Add(item);
    }

    public void InsertGameIconOverlayConfiguration(int index, GameIconOverlayConfigurationViewModel item)
    {
        InitializeGameIconOverlayConfiguration(item);
        GameIconOverlayConfigurations.Insert(index, item);
    }

    public void RemoveGameIconOverlayConfiguration(GameIconOverlayConfigurationViewModel item)
    {
        DeInitializeGameIconOverlayConfiguration(item);
        GameIconOverlayConfigurations.Remove(item);
    }

    public void ClearGameIconOverlayConfigurations()
    {
        foreach (var item in GameIconOverlayConfigurations.ToList())
        {
            RemoveGameIconOverlayConfiguration(item);
        }
    }

    public void AddPlatformIconConfiguration(PlatformIconConfigurationViewModel item)
    {
        InitializePlatformIconConfiguration(item);
        PlatformIconConfigurations.Add(item);
    }

    public void InsertPlatformIconConfiguration(int index, PlatformIconConfigurationViewModel item)
    {
        InitializePlatformIconConfiguration(item);
        PlatformIconConfigurations.Insert(index, item);
    }

    public void RemovePlatformIconConfiguration(PlatformIconConfigurationViewModel item)
    {
        DeInitializePlatformIconConfiguration(item);
        PlatformIconConfigurations.Remove(item);
    }

    public void ClearPlatformIconConfigurations()
    {
        foreach (var item in PlatformIconConfigurations.ToList())
        {
            RemovePlatformIconConfiguration(item);
        }
    }

    public void AddExtensionConfiguration(ExtensionConfigurationViewModel item)
    {
        InitializeExtensionConfiguration(item);
        ExtensionConfigurations.Add(item);
    }

    public void InsertExtensionConfiguration(int index, ExtensionConfigurationViewModel item)
    {
        InitializeExtensionConfiguration(item);
        ExtensionConfigurations.Insert(index, item);
    }

    public void RemoveExtensionConfiguration(ExtensionConfigurationViewModel item)
    {
        DeInitializeExtensionConfiguration(item);
        ExtensionConfigurations.Remove(item);
    }

    public void ClearExtensionConfigurations()
    {
        foreach (var item in ExtensionConfigurations.ToList())
        {
            RemoveExtensionConfiguration(item);
        }
    }

    public void AddScraperConfiguration(ScraperConfigurationViewModel item)
    {
        InitializeScraperConfiguration(item);
        ScraperConfigurations.Add(item);
    }

    public void InsertScraperConfiguration(int index, ScraperConfigurationViewModel item)
    {
        InitializeScraperConfiguration(item);
        ScraperConfigurations.Insert(index, item);
    }

    public void RemoveScraperConfiguration(ScraperConfigurationViewModel item)
    {
        DeInitializeScraperConfiguration(item);
        ScraperConfigurations.Remove(item);
    }

    public void ClearScraperConfigurations()
    {
        foreach (var item in ScraperConfigurations.ToList())
        {
            RemoveScraperConfiguration(item);
        }
    }

    public void AddUpscalerConfiguration(UpscalerConfigurationViewModel item)
    {
        InitializeUpscalerConfiguration(item);
        UpscalerConfigurations.Add(item);
    }

    public void InsertUpscalerConfiguration(int index, UpscalerConfigurationViewModel item)
    {
        InitializeUpscalerConfiguration(item);
        UpscalerConfigurations.Insert(index, item);
    }

    public void RemoveUpscalerConfiguration(UpscalerConfigurationViewModel item)
    {
        DeInitializeUpscalerConfiguration(item);
        UpscalerConfigurations.Remove(item);
    }

    public void ClearUpscalerConfigurations()
    {
        foreach (var item in UpscalerConfigurations.ToList())
        {
            RemoveUpscalerConfiguration(item);
        }
    }

    public void AddPlatformConfiguration(PlatformConfigurationViewModel item)
    {
        InitializePlatformConfiguration(item);
        PlatformConfigurations.Add(item);
    }

    public void InsertPlatformConfiguration(int index, PlatformConfigurationViewModel item)
    {
        InitializePlatformConfiguration(item);
        PlatformConfigurations.Insert(index, item);
    }

    public void RemovePlatformConfiguration(PlatformConfigurationViewModel item)
    {
        DeInitializePlatformConfiguration(item);
        PlatformConfigurations.Remove(item);
    }

    public void ClearPlatformConfigurations()
    {
        foreach (var item in PlatformConfigurations.ToList())
        {
            RemovePlatformConfiguration(item);
        }
    }

    public void AddReconstructorConfiguration(UpscalerConfigurationViewModel item)
    {
        InitializeReconstructorConfiguration(item);
        ReconstructorConfigurations.Add(item);
    }

    public void InsertReconstructorConfiguration(int index, UpscalerConfigurationViewModel item)
    {
        InitializeReconstructorConfiguration(item);
        ReconstructorConfigurations.Insert(index, item);
    }

    public void RemoveReconstructorConfiguration(UpscalerConfigurationViewModel item)
    {
        DeInitializeReconstructorConfiguration(item);
        ReconstructorConfigurations.Remove(item);
    }

    public void ClearReconstructorConfigurations()
    {
        foreach (var item in ReconstructorConfigurations.ToList())
        {
            RemoveReconstructorConfiguration(item);
        }
    }

    public void AddSpecificPlatform(SpecificPlatformViewModel item)
    {
        SpecificPlatforms.Add(item);
    }

    public void InsertSpecificPlatform(int index, SpecificPlatformViewModel item)
    {
        SpecificPlatforms.Insert(index, item);
    }

    public void RemoveSpecificPlatform(SpecificPlatformViewModel item)
    {
        SpecificPlatforms.Remove(item);
    }

    public void ClearSpecificPlatforms()
    {
        foreach (var item in SpecificPlatforms.ToList())
        {
            RemoveSpecificPlatform(item);
        }
    }

    public void AddSpecificGame(SpecificGameViewModel item)
    {
        InitializeSpecificGame(item);
        SpecificGames.Add(item);
    }

    public void InsertSpecificGame(int index, SpecificGameViewModel item)
    {
        InitializeSpecificGame(item);
        SpecificGames.Insert(index, item);
    }

    public void RemoveSpecificGame(SpecificGameViewModel item)
    {
        DeInitializeSpecificGame(item);
        SpecificGames.Remove(item);
    }

    public void ClearSpecificGames()
    {
        foreach (var item in SpecificGames.ToList())
        {
            RemoveSpecificGame(item);
        }
    }

    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void SelectAllSpecificPlatforms()
    {
        foreach (var platform in SpecificPlatforms)
        {
            platform.IsSelected = true;
        }

        BaseModel.SpecificPlatforms.Clear();
    }

    [RelayCommand]
    public void ClearAllSpecificPlatformSelections()
    {
        foreach (var platform in SpecificPlatforms)
        {
            platform.IsSelected = false;
        }
    }

    [RelayCommand]
    public void SelectAllSpecificGames()
    {
        foreach (var game in SpecificGames)
        {
            game.IsSelected = true;
        }

        BaseModel.SpecificGames.Clear();
    }

    [RelayCommand]
    public void ClearAllSpecificGameSelections()
    {
        foreach (var game in SpecificGames)
        {
            game.IsSelected = false;
        }
    }

    [RelayCommand]
    public void RequestShowConfiguraiton()
    {
        ShowConfiguraitonRequested?.Invoke(this, EventArgs.Empty);
    }

    protected IFileService FileService { get; private set; }
}
