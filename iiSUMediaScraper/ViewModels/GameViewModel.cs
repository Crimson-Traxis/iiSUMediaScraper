using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ViewModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for a game and its associated media.
/// </summary>
public partial class GameViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the file path of the game.
    /// </summary>
    [ObservableProperty]
    private string? path;

    /// <summary>
    /// Gets or sets the platform code for the game.
    /// </summary>
    [ObservableProperty]
    private string? platform;

    /// <summary>
    /// Gets or sets the folder name containing the game.
    /// </summary>
    [ObservableProperty]
    private string? folder;

    /// <summary>
    /// Gets or sets a value indicating whether the game is currently loading.
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// Gets or sets a value indicating whether the game is in demo mode.
    /// </summary>
    [ObservableProperty]
    private bool isInDemoMode;

    /// <summary>
    /// Gets or sets the demo mode icon image.
    /// </summary>
    [ObservableProperty]
    private Image? demoModeIcon;

    /// <summary>
    /// Gets or sets the demo mode title image.
    /// </summary>
    [ObservableProperty]
    private Image? demoModeTitle;

    /// <summary>
    /// Gets or sets the collection of demo mode hero images.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Image> demoModeHeros;

    /// <summary>
    /// Gets or sets the collection of demo mode slide images.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Image> demoModeSlides;

    /// <summary>
    /// Gets or sets the media context containing scraped media for the game.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFound))]
    private MediaContextViewModel? mediaContext;

    /// <summary>
    /// Gets or sets the configuration view model.
    /// </summary>
    [ObservableProperty]
    private ConfigurationViewModel configuration;

    /// <summary>
    /// Raised when a preview is requested for an image.
    /// </summary>
    public event EventHandler<GameImageEditRequestedEventArgs>? PreviewRequested;

    /// <summary>
    /// Raised when stopping a preview is requested for an image.
    /// </summary>
    public event EventHandler<GameImageEditRequestedEventArgs>? StopPreviewRequested;

    /// <summary>
    /// Raised when editing is requested for an image.
    /// </summary>
    public event EventHandler<GameImageEditRequestedEventArgs>? EditRequested;

    /// <summary>
    /// Raised when staging for apply is requested for the game.
    /// </summary>
    public event EventHandler<GameViewModel>? StageApplyGameRequested;

    /// <summary>
    /// Raised when the demo media changes 
    /// </summary>
    public event EventHandler<GameViewModel>? DemoMediaChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameViewModel"/> class.
    /// </summary>
    /// <param name="fileService">The file service.</param>
    /// <param name="scrapingService">The scraping service.</param>
    /// <param name="imageFormatterService">The image formatter service.</param>
    /// <param name="upscalerService">The upscaler service.</param>
    /// <param name="configuration">The configuration view model.</param>
    public GameViewModel(IFileService fileService,
                         IScrapingService scrapingService,
                         IImageFormatterService imageFormatterService,
                         IUpscalerService upscalerService,
                         ConfigurationViewModel configuration)
    {
        FileService = fileService;

        ScrapingService = scrapingService;

        ImageFormatterService = imageFormatterService;

        UpscalerService = upscalerService;

        Configuration = configuration;

        isInDemoMode = true;

        demoModeSlides = [];

        demoModeHeros = [];
    }

    partial void OnIsInDemoModeChanged(bool value)
    {
        if (MediaContext != null)
        {
            if (value)
            {
                MediaContext.ClearImageData();
            }
            else
            {
                _ = MediaContext.DownloadImageData();
            }
        }
    }

    private async void MediaContext_SelectionChanged(object? sender, EventArgs e)
    {
        await UpdateDemoModeMedia();
    }

    private void OnPreviewRequested(object? sender, ImageViewModel e)
    {
        PreviewRequested?.Invoke(this, new(this, e));
    }

    private void OnStopPreviewRequested(object? sender, ImageViewModel e)
    {
        StopPreviewRequested?.Invoke(this, new(this, e));
    }

    private void OnEditRequested(object? sender, ImageViewModel e)
    {
        EditRequested?.Invoke(this, new(this, e));
    }

    private async Task UpdateDemoModeIcon()
    {
        if (MediaContext != null)
        {
            if (MediaContext.Icons.FirstOrDefault(i => i.IsSelected) is ImageViewModel icon)
            {
                DemoModeIcon = await ImageFormatterService.FormatIcon(icon.BaseModel, Platform);
            }

            DemoMediaChanged?.Invoke(this, this);
        }

        OnPropertyChanged(nameof(IsFound));
    }

    private async Task UpdateDemoModeTitle()
    {
        if (MediaContext != null)
        {
            if (MediaContext.Logos.FirstOrDefault(l => l.IsSelected) is ImageViewModel logo)
            {
                DemoModeTitle = await ImageFormatterService.FormatTitle(logo.BaseModel);
            }

            if (MediaContext.Titles.FirstOrDefault(t => t.IsSelected) is ImageViewModel title)
            {
                DemoModeTitle = await ImageFormatterService.FormatTitle(title.BaseModel);
            }

            DemoMediaChanged?.Invoke(this, this);
        }

        OnPropertyChanged(nameof(IsFound));
    }

    private async Task UpdateDemoModeHeros()
    {
        if (MediaContext != null)
        {
            DemoModeHeros.Clear();

            foreach (ImageViewModel hero in MediaContext.Heros.Where(h => h.IsSelected).OfType<ImageViewModel>())
            {
                Image? image = await ImageFormatterService.FormatHero(hero.BaseModel);

                if (image != null)
                {
                    DemoModeHeros.Add(image);
                }
            }

            DemoMediaChanged?.Invoke(this, this);
        }

        OnPropertyChanged(nameof(IsFound));
    }

    private async Task UpdateDemoModeSlides()
    {
        if (MediaContext != null)
        {
            DemoModeSlides.Clear();

            foreach (ImageViewModel slide in MediaContext.Slides.Where(s => s.IsSelected).OfType<ImageViewModel>())
            {
                Image? image = await ImageFormatterService.FormatSlide(slide.BaseModel);

                if (image != null)
                {
                    DemoModeSlides.Add(image);
                }
            }

            DemoMediaChanged?.Invoke(this, this);
        }

        OnPropertyChanged(nameof(IsFound));
    }

    private void InitializeMediaContext(MediaContextViewModel mediaContext)
    {
        mediaContext.PreviewRequested += OnPreviewRequested;
        mediaContext.StopPreviewRequested += OnStopPreviewRequested;
        mediaContext.EditRequested += OnEditRequested;
        mediaContext.SelectionChanged += MediaContext_SelectionChanged;
    }

    private void DeInitializeMediaContext(MediaContextViewModel mediaContext)
    {
        mediaContext.PreviewRequested -= OnPreviewRequested;
        mediaContext.StopPreviewRequested -= OnStopPreviewRequested;
        mediaContext.EditRequested -= OnEditRequested;
        mediaContext.SelectionChanged -= MediaContext_SelectionChanged;
    }

    public async Task UpdateDemoModeMedia()
    {
        if (MediaContext != null)
        {
            List<Task> tasks =
            [
                UpdateDemoModeIcon(),
                UpdateDemoModeTitle(),
                UpdateDemoModeHeros(),
                UpdateDemoModeSlides()
            ];

            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Stages the game for applying media assets.
    /// </summary>
    [RelayCommand]
    public async Task StageApply()
    {
        StageApplyGameRequested?.Invoke(this, this);
    }

    /// <summary>
    /// Applies the scraped media to the game's asset folder.
    /// </summary>
    public async Task Apply()
    {
        if (IsFound)
        {
            if (!string.IsNullOrWhiteSpace(Configuration.UnfoundMediaMovePath) &&
                !string.IsNullOrWhiteSpace(Configuration.GamesPath) &&
                !string.IsNullOrWhiteSpace(Path) &&
                !string.IsNullOrWhiteSpace(Folder))
            {
                if (Path.Contains(Configuration.UnfoundMediaMovePath))
                {
                    await FileService.MoveFile(Path, FileService.CombinePath(Configuration.GamesPath, Folder, FileService.CleanFileName(Name), NameWithExtension));
                }
            }
        }
        else
        {
            if (Configuration.IsMoveToUnfoundGamesFolder &&
                !string.IsNullOrWhiteSpace(Configuration.UnfoundMediaMovePath) &&
                !string.IsNullOrWhiteSpace(Path) &&
                !string.IsNullOrWhiteSpace(Folder))
            {
                if (!Path.Contains(Configuration.UnfoundMediaMovePath))
                {
                    await FileService.MoveFile(Path, FileService.CombinePath(Configuration.UnfoundMediaMovePath, Folder, FileService.CleanFileName(Name), NameWithExtension));
                }
            }
        }

        if (!string.IsNullOrEmpty(Configuration.ApplyAssetPath) &&
            !string.IsNullOrEmpty(Folder))
        {
            if (IsFound || Configuration.IsApplyUnfoundGames)
            {
                var path = FileService.CombinePath(Configuration.ApplyAssetPath, Folder, FileService.CleanFileName(Name));

                await FileService.CreateDirectory(path);

                if (Configuration.IsApplyIcon && !string.IsNullOrWhiteSpace(Configuration.IconNameFormat) && DemoModeIcon != null)
                {
                    await FileService.SaveBytes(path, $"{Configuration.IconNameFormat}.png", DemoModeIcon.Bytes);
                }

                if (Configuration.IsApplyTitle && !string.IsNullOrWhiteSpace(Configuration.TitleNameFormat) && DemoModeTitle != null)
                {
                    await FileService.SaveBytes(path, $"{Configuration.TitleNameFormat}.png", DemoModeTitle.Bytes);
                }

                if (Configuration.IsApplyHeros && !string.IsNullOrWhiteSpace(Configuration.HeroNameFormat))
                {
                    await FileService.DeleteFiles(path, $"{Configuration.HeroNameFormat}.*");

                    for (int i = 0; i < DemoModeHeros.Count; i++)
                    {
                        await FileService.SaveBytes(path, $"{Configuration.HeroNameFormat}{i + 1}.png", DemoModeHeros[i].Bytes);
                    }
                }

                if (Configuration.IsApplySlides && !string.IsNullOrWhiteSpace(Configuration.SlideNameFormat))
                {
                    await FileService.DeleteFiles(path, $"{Configuration.HeroNameFormat}.*");

                    for (int i = 0; i < DemoModeSlides.Count; i++)
                    {
                        await FileService.SaveBytes(path, $"{Configuration.HeroNameFormat}{i + 1}.png", DemoModeSlides[i].Bytes);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Scrapes media for the game from configured scraping sources.
    /// </summary>
    public async Task Scrape()
    {
        DemoModeHeros.Clear();

        DemoModeSlides.Clear();

        if (!string.IsNullOrWhiteSpace(Path) && !string.IsNullOrWhiteSpace(Platform))
        {
            IsLoading = true;

            MediaContext mediaContext = await ScrapingService.GetMedia(Platform, Name);

            if (MediaContext != null)
            {
                DeInitializeMediaContext(MediaContext);
            }

            if (mediaContext != null)
            {
                foreach (Image icon in mediaContext.Icons)
                {
                    icon.Crop = await ImageFormatterService.SmartCropIcon(icon);
                }

                foreach (Image logo in mediaContext.Logos)
                {
                    logo.Crop = await ImageFormatterService.SmartCropLogo(logo);
                }

                foreach (Image title in mediaContext.Titles)
                {
                    title.Crop = await ImageFormatterService.SmartCropTitle(title);
                }

                foreach (Image hero in mediaContext.Heros.OfType<Image>())
                {
                    hero.Crop = await ImageFormatterService.SmartCropHero(hero);
                }

                foreach (Image slide in mediaContext.Slides.OfType<Image>())
                {
                    slide.Crop = await ImageFormatterService.SmartCropSlide(slide);
                }

                MediaContext = new MediaContextViewModel(mediaContext, ScrapingService, ImageFormatterService, UpscalerService, FileService, Configuration);

                InitializeMediaContext(MediaContext);

                // Set selections

                await UpdateDemoModeMedia();

                if (IsInDemoMode)
                {
                    MediaContext.ClearImageData();
                }
            }

            IsLoading = false;
        }
    }

    /// <summary>
    /// Gets the name of the game without file extension.
    /// </summary>
    public string Name => FileService?.GetFileNameWithoutExtension(Path ?? "") ?? "";

    /// <summary>
    /// Gets the name of the game with file extension.
    /// </summary>
    public string NameWithExtension => FileService?.GetFileName(Path ?? "") ?? "";

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
    /// Gets a value indicating whether the game has been found with all required media.
    /// </summary>
    public bool IsFound
    {
        get
        {
            if (MediaContext != null)
            {
                bool isfound = true;

                if (Configuration.IsUnfoundMediaIfNoIcons)
                {
                    isfound &= MediaContext.Icons.Any();
                }

                if (Configuration.IsUnfoundMediaIfNoLogos)
                {
                    isfound &= MediaContext.Logos.Any();
                }

                if (Configuration.IsUnfoundMediaIfNoTitles)
                {
                    isfound &= MediaContext.Titles.Any();
                }

                if (Configuration.IsUnfoundMediaIfNoHeros)
                {
                    isfound &= MediaContext.Heros.Any();
                }

                if (Configuration.IsUnfoundMediaIfNoSlides)
                {
                    isfound &= MediaContext.Slides.Any();
                }

                return isfound;
            }

            return true;
        }
    }

    /// <summary>
    /// Provides data for game image edit request events.
    /// </summary>
    public class GameImageEditRequestedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameImageEditRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="gameViewModel">The game view model.</param>
        /// <param name="imageViewModel">The image view model.</param>
        public GameImageEditRequestedEventArgs(GameViewModel gameViewModel, ImageViewModel imageViewModel)
        {
            GameViewModel = gameViewModel;
            ImageViewModel = imageViewModel;
        }

        /// <summary>
        /// Gets the game view model.
        /// </summary>
        public GameViewModel GameViewModel { get; private set; }

        /// <summary>
        /// Gets the image view model.
        /// </summary>
        public ImageViewModel ImageViewModel { get; private set; }
    }
}
