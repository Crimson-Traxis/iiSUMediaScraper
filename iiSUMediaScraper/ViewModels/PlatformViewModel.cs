using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for a gaming platform and its associated games.
/// </summary>
public partial class PlatformViewModel : ObservableObject
{
    /// <summary>
    /// Semaphore for controlling concurrent scraping operations.
    /// </summary>
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Gets or sets a value indicating whether the platform is loading.
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// Gets or sets the count of scraped games.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Progress))]
    private int scrapedGamesCount;

    /// <summary>
    /// Gets or sets a value indicating whether an image is being edited.
    /// </summary>
    [ObservableProperty]
    private bool isEditingImage;

    /// <summary>
    /// Gets or sets the image view model currently being edited.
    /// </summary>
    [ObservableProperty]
    private ImageViewModel? editImage;

    /// <summary>
    /// Gets or sets the original image view model before editing.
    /// </summary>
    [ObservableProperty]
    private ImageViewModel? originalImage;

    /// <summary>
    /// Gets or sets the game associated with the image being edited.
    /// </summary>
    [ObservableProperty]
    private GameViewModel? editImageGame;

    /// <summary>
    /// Gets or sets the platform configuration.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Code))]
    private PlatformConfigurationViewModel platformConfiguration;

    /// <summary>
    /// Gets or sets the configuration view model.
    /// </summary>
    [ObservableProperty]
    private ConfigurationViewModel configuration;

    /// <summary>
    /// Gets or sets the collection of games for this platform.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<GameViewModel> games;

    /// <summary>
    /// Gets or sets the collection of folders containing games.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> folders;

    /// <summary>
    /// Gets or sets the collection of image upscale configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ImageUpscaleConfigurationViewModel> imageUpscaleConfigurations;

    /// <summary>
    /// Raised when editing is requested for an image.
    /// </summary>
    public event EventHandler<ImageViewModel> EditRequested;

    /// <summary>
    /// Raised when staging for apply is requested for games.
    /// </summary>
    public event EventHandler<IEnumerable<GameViewModel>>? StageApplyGamesRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformViewModel"/> class.
    /// </summary>
    /// <param name="fileService">The file service.</param>
    /// <param name="scrapingService">The scraping service.</param>
    /// <param name="mediaFormatterService">The media formatter service.</param>
    /// <param name="upscalerService">The upscaler service.</param>
    /// <param name="configuration">The configuration view model.</param>
    /// <param name="platformConfiguration">The platform configuration.</param>
    /// <param name="semaphore">The semaphore for controlling concurrency.</param>
    public PlatformViewModel(IFileService fileService,
                             IScrapingService scrapingService,
                             IMediaFormatterService mediaFormatterService,
                             IUpscalerService upscalerService,
                             IDownloader downloader,
                             ConfigurationViewModel configuration,
                             PlatformConfigurationViewModel platformConfiguration,
                             SemaphoreSlim semaphore)
    {
        FileService = fileService;

        Configuration = configuration;

        ScrapingService = scrapingService;

        MediaFormatterService = mediaFormatterService;

        Downloader = downloader;

        UpscalerService = upscalerService;

        PlatformConfiguration = platformConfiguration;

        _semaphore = semaphore;

        games = [];

        folders = [];

        imageUpscaleConfigurations = [];
    }

    /// <summary>
    /// Called when the PlatformConfiguration property changes.
    /// </summary>
    /// <param name="oldValue">The old platform configuration.</param>
    /// <param name="newValue">The new platform configuration.</param>
    partial void OnPlatformConfigurationChanged(PlatformConfigurationViewModel? oldValue, PlatformConfigurationViewModel newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= Platform_PropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += Platform_PropertyChanged;
        }
    }

    /// <summary>
    /// Handles platform property changed events to update the Code property.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The property changed event args.</param>
    private void Platform_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Code));
    }

    /// <summary>
    /// Called when the EditImage property changes.
    /// </summary>
    /// <param name="oldValue">The old edit image.</param>
    /// <param name="newValue">The new edit image.</param>
    partial void OnEditImageChanged(ImageViewModel? oldValue, ImageViewModel? newValue)
    {
        if (oldValue != null)
        {
            oldValue.UpscaleCompleted -= Image_UpscaleCompleted;
        }

        ImageUpscaleConfigurations.Clear();

        if (newValue != null)
        {
            newValue.UpscaleCompleted += Image_UpscaleCompleted;

            foreach (var configuration in Configuration.UpscalerConfigurations)
            {
                ImageUpscaleConfigurations.Add(new ImageUpscaleConfigurationViewModel(newValue, configuration));
            }
        }
    }

    /// <summary>
    /// Handles the upscale completed event for an image.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The upscaled image view model.</param>
    private void Image_UpscaleCompleted(object? sender, ImageViewModel e)
    {
        EditRequested?.Invoke(this, e);
    }

    /// <summary>
    /// Creates a copy of an image view model with independent data.
    /// </summary>
    /// <param name="imageViewModel">The image view model to copy.</param>
    /// <returns>A new image view model with copied data.</returns>
    private ImageViewModel CreateCopy(ImageViewModel imageViewModel)
    {
        return new ImageViewModel(new Image()
        {
            LocalPath = imageViewModel.LocalPath,
            Width = imageViewModel.Width,
            Height = imageViewModel.Height,
            Crop = imageViewModel.Crop != null ? new Crop()
            {
                Left = imageViewModel.Crop.Left,
                Top = imageViewModel.Crop.Top,
                Width = imageViewModel.Crop.Width,
                Height = imageViewModel.Crop.Height,
            } : null,
        }, imageViewModel.MediaType, MediaFormatterService, UpscalerService, Downloader, Configuration);
    }

    /// <summary>
    /// Handles preview requested events from games.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The game image edit request event args.</param>
    private void OnPreviewRequested(object? sender, GameViewModel.GameImageEditRequestedEventArgs e)
    {
        if (!IsEditingImage)
        {
            OriginalImage = e.ImageViewModel;

            EditImage = CreateCopy(e.ImageViewModel);

            EditImageGame = e.GameViewModel;
        }
    }

    /// <summary>
    /// Handles stop preview requested events from games.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The game image edit request event args.</param>
    private void OnStopPreviewRequested(object? sender, GameViewModel.GameImageEditRequestedEventArgs e)
    {
        if (!IsEditingImage)
        {
            OriginalImage = null;

            EditImage = null;

            EditImageGame = null;

            IsEditingImage = false;
        }
    }

    /// <summary>
    /// Handles edit requested events from games.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The game image edit request event args.</param>
    private void OnEditRequested(object? sender, GameViewModel.GameImageEditRequestedEventArgs e)
    {
        IsEditingImage = true;

        OriginalImage = e.ImageViewModel;

        EditImage = CreateCopy(e.ImageViewModel);

        EditImageGame = e.GameViewModel;

        EditRequested?.Invoke(this, e.ImageViewModel);
    }

    /// <summary>
    /// Handles stage apply game requested events.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The game view model to stage.</param>
    private void StageApplyGameRequested(object? sender, GameViewModel e)
    {
        StageApplyGamesRequested?.Invoke(this, [e]);
    }

    /// <summary>
    /// Initializes event handlers for a game.
    /// </summary>
    /// <param name="game">The game to initialize.</param>
    private void InitializeGame(GameViewModel game)
    {
        game.PreviewRequested += OnPreviewRequested;
        game.StopPreviewRequested += OnStopPreviewRequested;
        game.EditRequested += OnEditRequested;
        game.StageApplyGameRequested += StageApplyGameRequested;
    }

    /// <summary>
    /// Removes event handlers from a game.
    /// </summary>
    /// <param name="game">The game to de-initialize.</param>
    private void DeInitializeGame(GameViewModel game)
    {
        game.PreviewRequested -= OnPreviewRequested;
        game.StopPreviewRequested -= OnStopPreviewRequested;
        game.EditRequested -= OnEditRequested;
        game.StageApplyGameRequested -= StageApplyGameRequested;
    }

    /// <summary>
    /// Adds a game to the platform.
    /// </summary>
    /// <param name="game">The game to add.</param>
    public void AddGame(GameViewModel game)
    {
        InitializeGame(game);

        Games.Add(game);
    }

    /// <summary>
    /// Removes a game from the platform.
    /// </summary>
    /// <param name="game">The game to remove.</param>
    public void RemoveGame(GameViewModel game)
    {
        DeInitializeGame(game);
        Games.Remove(game);
    }

    /// <summary>
    /// Clears all games from the platform.
    /// </summary>
    public void ClearGames()
    {
        foreach (GameViewModel? game in Games.ToList())
        {
            RemoveGame(game);
        }
    }

    /// <summary>
    /// Scrapes media for a single game with semaphore-controlled concurrency.
    /// </summary>
    /// <param name="game">The game to scrape.</param>
    public async Task OnScrapeGame(GameViewModel game)
    {
        await _semaphore.WaitAsync();

        try
        {
            await game.Scrape();

            ScrapedGamesCount++;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Finds games in the configured folders for this platform.
    /// </summary>
    public async Task FindGames()
    {
        IsLoading = true;

        ClearGames();

        ScrapedGamesCount = 0;

        foreach (string folder in Folders)
        {
            foreach (ExtensionConfigurationViewModel? extensionConfiguraion in Configuration.ExtensionConfigurations.Where(e => e.Platform == PlatformConfiguration.Code))
            {
                foreach (var exension in extensionConfiguraion.Extensions)
                {
                    foreach (var file in await FileService.GetFiles(folder, $"*{exension}"))
                    {
                        var game = new GameViewModel(FileService, ScrapingService, MediaFormatterService, UpscalerService, Downloader, Configuration)
                        {
                            Path = file,

                            Folder = FileService.GetFolderName(folder),

                            Platform = PlatformConfiguration.Code
                        };

                        if (Configuration.SpecificGames.Any(g => g.IsSelected))
                        {
                            if (Configuration.SpecificGames.Any(g => g.IsSelected && g.Path == game.Path))
                            {
                                AddGame(game);
                            }
                        }
                        else
                        {
                            AddGame(game);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Scrapes media for all games on this platform.
    /// </summary>
    public async Task Scrape()
    {
        ScrapedGamesCount = 0;

        IsLoading = true;

        foreach (GameViewModel game in Games)
        {
            game.IsLoading = true;
        }

        List<Task> tasks = [];

        foreach (GameViewModel game in Games)
        {
            tasks.Add(OnScrapeGame(game));

            tasks.Add(game.Format());
        }

        await Task.WhenAll(tasks);

        IsLoading = false;
    }

    /// <summary>
    /// Stops editing the current image.
    /// </summary>
    [RelayCommand]
    public async Task StopEdit()
    {
        IsEditingImage = false;
        EditImage = null;
    }

    /// <summary>
    /// Saves the edited image changes to the original image.
    /// </summary>
    [RelayCommand]
    public async Task Save()
    {
        if (OriginalImage != null && EditImage != null)
        {
            OriginalImage.LocalPath = EditImage.LocalPath;
            OriginalImage.Width = EditImage.Width;
            OriginalImage.Height = EditImage.Height;

            if (EditImage.Crop != null)
            {
                var crop = new Crop
                {
                    Top = EditImage.Crop.Top,
                    Left = EditImage.Crop.Left,
                    Width = EditImage.Crop.Width,
                    Height = EditImage.Crop.Height
                };

                OriginalImage.Crop = new CropViewModel(crop);
            }
        }
    }

    /// <summary>
    /// Stages all games on this platform for applying.
    /// </summary>
    [RelayCommand]
    public async Task StageApply()
    {
        StageApplyGamesRequested?.Invoke(this, Games);
    }

    /// <summary>
    /// Gets the file service.
    /// </summary>
    protected IFileService FileService { get; private set; }

    /// <summary>
    /// Gets the scraping service.
    /// </summary>
    protected IScrapingService ScrapingService { get; private set; }

    /// <summary>
    /// Gets the meida formatter service.
    /// </summary>
    protected IMediaFormatterService MediaFormatterService { get; private set; }

    /// <summary>
    /// Gets the upscaler service.
    /// </summary>
    protected IUpscalerService UpscalerService { get; private set; }

    /// <summary>
    /// Gets the downloader service.
    /// </summary>
    protected IDownloader Downloader { get; private set; }

    /// <summary>
    /// Gets the scraping progress percentage for this platform.
    /// </summary>
    public int Progress => Games.Count > 0 ? (int)((ScrapedGamesCount / (double)Games.Count) * 100) : 0;

    /// <summary>
    /// Gets the platform code derived from the platform name.
    /// </summary>
    public string Code
    {
        get
        {
            var name = PlatformConfiguration.Name;

            if (string.IsNullOrWhiteSpace(name))
                return "";

            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return words.Length >= 2
                ? $"{words[0][0]}{words[1][0]}".ToUpperInvariant()
                : name.Length >= 2
                    ? name[..2].ToUpperInvariant()
                    : name.ToUpperInvariant();
        }
    }
}
