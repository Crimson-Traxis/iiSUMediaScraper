using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Scraping.SteamGridDb;
using iiSUMediaScraper.Services;
using iiSUMediaScraper.ViewModels.Configurations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using WinUIEx;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for a game and its associated media.
/// </summary>
public partial class GameViewModel : ObservableObject
{
    private CancellationTokenSource? _iconCancellationTokenSource;

    private CancellationTokenSource? _titleCancellationTokenSource;

    private CancellationTokenSource? _herosCancellationTokenSource;

    private CancellationTokenSource? _slidesCancellationTokenSource;

    private CancellationTokenSource? _cancelUrlFetchCancellationTokenSource;

    private TaskCompletionSource _scrapTaskCompletionSource;

    private TaskCompletionSource? _formatIconTaskCompletionSource;

    private TaskCompletionSource? _formatTitleTaskCompletionSource;

    private TaskCompletionSource? _formatHerosTaskCompletionSource;

    private TaskCompletionSource? _formatSlidesTaskCompletionSource;

    private TaskCompletionSource? _formatMusicTaskCompletionSource;

    /// <summary>
    /// Gets or sets the is hover.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotIsFoundOrIsHover))]
    private bool isHover;

    /// <summary>
    /// Gets or sets the is importing url.
    /// </summary>
    [ObservableProperty]
    private bool isImportingUrl;

    /// <summary>
    /// Gets or sets the is entering url.
    /// </summary>
    [ObservableProperty]
    private bool isEnteringUrl;

    /// <summary>
    /// Gets or sets the url type;
    /// </summary>
    [ObservableProperty]
    private string urlType;

    /// <summary>
    /// Gets or sets the url;
    /// </summary>
    [ObservableProperty]
    private string url;

    /// <summary>
    /// Gets or sets the is clear items for when entering a url.
    /// </summary>
    [ObservableProperty]
    private bool isClearItems;

    /// <summary>
    /// Gets or sets the view index of the game.
    /// </summary>
    [ObservableProperty]
    private int viewIndex;

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
    /// Gets or sets a value indication wheather the icon is actively being formatted.
    /// </summary>
    [ObservableProperty]
    private bool isFormattingIcon;

    /// <summary>
    /// Gets or sets a value indication wheather the title is actively being formatted.
    /// </summary>
    [ObservableProperty]
    private bool isFormattingTitle;

    /// <summary>
    /// Gets or sets a value indication wheather the heros are actively being formatted.
    /// </summary>
    [ObservableProperty]
    private bool isFormattingHeros;

    /// <summary>
    /// Gets or sets a value indication wheather the slides are actively being formatted.
    /// </summary>
    [ObservableProperty]
    private bool isFormattingSlides;

    /// <summary>
    /// Gets or sets a value indicating whether the game is in demo mode.
    /// </summary>
    [ObservableProperty]
    private bool isInDemoMode;

    /// <summary>
    /// Gets or sets a the selected demo mode music.
    /// </summary>
    [ObservableProperty]
    private MusicViewModel? selectedDemoModeMusic;

    /// <summary>
    /// Gets or sets the demo mode icon image.
    /// </summary>
    [ObservableProperty]
    private ImageViewModel? demoModeIcon;

    /// <summary>
    /// Gets or sets the demo mode title image.
    /// </summary>
    [ObservableProperty]
    private ImageViewModel? demoModeTitle;

    /// <summary>
    /// Gets or sets the collection of demo mode hero images.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MediaViewModel> demoModeHeros;

    /// <summary>
    /// Gets or sets the collection of demo mode slide images.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MediaViewModel> demoModeSlides;

    /// <summary>
    /// Gets or sets the collection of demo mode music.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MusicViewModel> demoModeMusic;

    /// <summary>
    /// Gets or sets the media context containing scraped media for the game.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFound))]
    [NotifyPropertyChangedFor(nameof(NotIsFoundOrIsHover))]
    private MediaContextViewModel? mediaContext;

    /// <summary>
    /// Gets or sets the configuration view model.
    /// </summary>
    [ObservableProperty]
    private ConfigurationViewModel configuration;

    /// <summary>
    /// Gets or sets the playing music view model.
    /// </summary>
    [ObservableProperty]
    private MusicViewModel? playingMusic;

    /// <summary>
    /// Gets or sets the playing video view model.
    /// </summary>
    [ObservableProperty]
    private VideoViewModel? playingVideo;

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
    /// <param name="mediaFormatterService">The media formatter service.</param>
    /// <param name="upscalerService">The upscaler service.</param>
    /// <param name="configuration">The configuration view model.</param>
    public GameViewModel(IFileService fileService,
                         IScrapingService scrapingService,
                         IMediaFormatterService mediaFormatterService,
                         IUpscalerService upscalerService,
                         IDownloader downloader,
                         ConfigurationViewModel configuration)
    {
        FileService = fileService;

        ScrapingService = scrapingService;

        MediaFormatterService = mediaFormatterService;

        UpscalerService = upscalerService;

        Downloader = downloader;

        Configuration = configuration;

        isInDemoMode = true;

        demoModeSlides = [];

        demoModeHeros = [];

        demoModeMusic = [];

        _scrapTaskCompletionSource = new TaskCompletionSource();
    }

    partial void OnIsInDemoModeChanged(bool value)
    {
        if (!value && MediaContext != null)
        {
            var smartCropBackgroundTasks = new List<Task>();

            smartCropBackgroundTasks.AddRange(MediaContext.Icons.Where(i => !i.HasCrop).Select(i => OnCropMedia(i, MediaType.Icon)));

            smartCropBackgroundTasks.AddRange(MediaContext.Logos.Where(l => !l.HasCrop).Select(l => OnCropMedia(l, MediaType.Logo)));

            smartCropBackgroundTasks.AddRange(MediaContext.Titles.Where(t => !t.HasCrop).Select(t => OnCropMedia(t, MediaType.Title)));

            smartCropBackgroundTasks.AddRange(MediaContext.Heros.Where(h => !h.HasCrop).OfType<ImageViewModel>().Select(h => OnCropMedia(h, MediaType.Hero)));

            smartCropBackgroundTasks.AddRange(MediaContext.Slides.Where(s => !s.HasCrop).OfType<ImageViewModel>().Select(s => OnCropMedia(s, MediaType.Hero)));

            _ = Task.WhenAll(smartCropBackgroundTasks);
        }
    }

    /// <summary>
    /// Handles the selection changed event from the media context.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The media view model whose selection changed.</param>
    private async void OnSelectionChanged(object? sender, MediaViewModel e)
    {
        if (e is ImageViewModel imageViewModel)
        {

        }
    }

    /// <summary>
    /// Handles the preview requested event from the media context.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The image view model to preview.</param>
    private void OnPreviewRequested(object? sender, ImageViewModel e)
    {
        PreviewRequested?.Invoke(this, new(this, e));
    }

    /// <summary>
    /// Handles the stop preview requested event from the media context.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The image view model to stop previewing.</param>
    private void OnStopPreviewRequested(object? sender, ImageViewModel e)
    {
        StopPreviewRequested?.Invoke(this, new(this, e));
    }

    /// <summary>
    /// Handles the edit requested event from the media context.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The image view model to edit.</param>
    private void OnEditRequested(object? sender, ImageViewModel e)
    {
        EditRequested?.Invoke(this, new(this, e));
    }

    /// <summary>
    /// Handles the video play requested event from the media context.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnVideoPlayRequested(object? sender, EventArgs e)
    {
        if (sender is VideoViewModel videoViewModel)
        {
            PlayingVideo = videoViewModel;
        }
    }

    /// <summary>
    /// Handles the music play requested event from the media context.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnMusicPlayRequested(object? sender, EventArgs e)
    {
        if (sender is MusicViewModel musicViewModel)
        {
            PlayingMusic = musicViewModel;
        }
    }

    /// <summary>
    /// Handles the icon selection changed event to update the demo mode icon.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The image view model whose selection changed.</param>
    private async void OnIconSelectionChanged(object? sender, ImageViewModel e)
    {
        await UpdateDemoModeIcon();
    }

    /// <summary>
    /// Handles the title selection changed event to update the demo mode title.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The image view model whose selection changed.</param>
    private async void OnTitleSelectionChanged(object? sender, ImageViewModel e)
    {
        await UpdateDemoModeTitle();
    }

    /// <summary>
    /// Handles the logo selection changed event to update the demo mode title.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The image view model whose selection changed.</param>
    private async void OnLogoSelectionChanged(object? sender, ImageViewModel e)
    {
        await UpdateDemoModeTitle();
    }

    /// <summary>
    /// Handles the hero selection changed event to update the demo mode heroes.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The image view model whose selection changed.</param>
    private async void OnHeroSelectionChanged(object? sender, ImageViewModel e)
    {
        await UpdateDemoModeHeros();
    }

    /// <summary>
    /// Handles the slide selection changed event to update the demo mode slides.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The image view model whose selection changed.</param>
    private async void OnSlideSelectionChanged(object? sender, ImageViewModel e)
    {
        await UpdateDemoModeSlides();
    }

    /// <summary>
    /// Handles the music selection changed event to update the demo mode music.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The music view model whose selection changed.</param>
    private async void OnMusicSelectionChanged(object? sender, MusicViewModel e)
    {
        await e.Download();

        await UpdateDemoModeMusic();
    }

    /// <summary>
    /// Handles the video selection changed event to update demo mode heroes and slides.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The video view model whose selection changed.</param>
    private async void OnVideoSelectionChanged(object? sender, VideoViewModel e)
    {
        await e.Download();

        await UpdateDemoModeHeros();
        await UpdateDemoModeSlides();
    }

    /// <summary>
    /// Initializes event handlers for a media context.
    /// </summary>
    /// <param name="mediaContext">The media context to initialize.</param>
    private void InitializeMediaContext(MediaContextViewModel mediaContext)
    {
        mediaContext.PreviewRequested += OnPreviewRequested;
        mediaContext.StopPreviewRequested += OnStopPreviewRequested;
        mediaContext.EditRequested += OnEditRequested;
        mediaContext.SelectionChanged += OnSelectionChanged;
        mediaContext.MusicPlayRequested += OnMusicPlayRequested;
        mediaContext.VideoPlayRequested += OnVideoPlayRequested;

        mediaContext.IconSelectionChanged += OnIconSelectionChanged;
        mediaContext.TitleSelectionChanged += OnTitleSelectionChanged;
        mediaContext.LogoSelectionChanged += OnLogoSelectionChanged;
        mediaContext.HeroSelectionChanged += OnHeroSelectionChanged;
        mediaContext.SlideSelectionChanged += OnSlideSelectionChanged;
        mediaContext.MusicSelectionChanged += OnMusicSelectionChanged;
        mediaContext.VideoSelectionChanged += OnVideoSelectionChanged;
    }

    /// <summary>
    /// Removes event handlers from a media context.
    /// </summary>
    /// <param name="mediaContext">The media context to de-initialize.</param>
    private void DeInitializeMediaContext(MediaContextViewModel mediaContext)
    {
        mediaContext.PreviewRequested -= OnPreviewRequested;
        mediaContext.StopPreviewRequested -= OnStopPreviewRequested;
        mediaContext.EditRequested -= OnEditRequested;
        mediaContext.SelectionChanged -= OnSelectionChanged;
        mediaContext.MusicPlayRequested -= OnMusicPlayRequested;
        mediaContext.VideoPlayRequested -= OnVideoPlayRequested;

        mediaContext.IconSelectionChanged -= OnIconSelectionChanged;
        mediaContext.TitleSelectionChanged -= OnTitleSelectionChanged;
        mediaContext.LogoSelectionChanged -= OnLogoSelectionChanged;
        mediaContext.HeroSelectionChanged -= OnHeroSelectionChanged;
        mediaContext.SlideSelectionChanged -= OnSlideSelectionChanged;
        mediaContext.MusicSelectionChanged -= OnMusicSelectionChanged;
        mediaContext.VideoSelectionChanged -= OnVideoSelectionChanged;
    }

    private async Task OnCropMedia(ImageViewModel imageViewModel, MediaType mediaType)
    {
        if(imageViewModel.Crop == null)
        {
            Crop? crop = null;

            imageViewModel.IsLoading = true;

            switch (mediaType)
            {
                case MediaType.Icon:
                    crop = await MediaFormatterService.SmartCropIcon(imageViewModel.BaseModel);
                    break;
                case MediaType.Title:
                    crop = await MediaFormatterService.SmartCropTitle(imageViewModel.BaseModel);
                    break;
                case MediaType.Logo:
                    crop = await MediaFormatterService.SmartCropLogo(imageViewModel.BaseModel);
                    break;
                case MediaType.Hero:
                    crop = await MediaFormatterService.SmartCropHero(imageViewModel.BaseModel);
                    break;
                case MediaType.Slide:
                    crop = await MediaFormatterService.SmartCropSlide(imageViewModel.BaseModel);
                    break;
            }

            if (crop != null)
            {
                imageViewModel.Crop = new CropViewModel(crop);
            }

            imageViewModel.IsLoading = false;
        }
    }

    /// <summary>
    /// Removes parenthetical region/version info from a game name.
    /// Example: "Super Mario Bros (USA)" becomes "Super Mario Bros"
    /// </summary>
    /// <param name="name">The game name to clean.</param>
    /// <returns>Cleaned game name.</returns>
    protected string CleanName(string name)
    {
        // Remove only trailing parenthetical groups (region info, version, etc.)
        string pattern = @"(\s*\([^)]*\))+$";
        name = Regex.Replace(name, pattern, string.Empty).Trim();

        return name;
    }

    /// <summary>
    /// Updates the demo mode icon by formatting the selected icon.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateDemoModeIcon()
    {
        if (MediaContext != null)
        {
            _formatIconTaskCompletionSource = new TaskCompletionSource();

            if (_iconCancellationTokenSource != null)
            {
                await _iconCancellationTokenSource.CancelAsync();
            }

            _iconCancellationTokenSource = new CancellationTokenSource();

            var tokenSource = _iconCancellationTokenSource;

            IsFormattingIcon = true;

            if (MediaContext.Icons.FirstOrDefault(i => i.IsSelected) is ImageViewModel icon && !string.IsNullOrEmpty(Platform))
            {
                var alteredIcon = await MediaFormatterService.FormatIcon(icon.BaseModel, Platform, tokenSource.Token);

                if (!tokenSource.Token.IsCancellationRequested && alteredIcon != null)
                {
                    DemoModeIcon = MediaContext.CreateIcon(alteredIcon);
                }
            }

            if (!tokenSource.Token.IsCancellationRequested)
            {
                DemoMediaChanged?.Invoke(this, this);

                IsFormattingIcon = false;

                _formatIconTaskCompletionSource.SetResult();
            }
        }

        OnPropertyChanged(nameof(IsFound));
        OnPropertyChanged(nameof(NotIsFoundOrIsHover));
    }

    /// <summary>
    /// Updates the demo mode title by formatting the selected title or logo.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateDemoModeTitle()
    {
        if (MediaContext != null)
        {
            _formatTitleTaskCompletionSource = new TaskCompletionSource();

            if (_titleCancellationTokenSource != null)
            {
                await _titleCancellationTokenSource.CancelAsync();
            }

            _titleCancellationTokenSource = new CancellationTokenSource();

            var tokenSource = _titleCancellationTokenSource;

            IsFormattingTitle = true;

            if (MediaContext.Logos.FirstOrDefault(l => l.IsSelected) is ImageViewModel logo)
            {
                var alteredTitle = await MediaFormatterService.FormatTitle(logo.BaseModel, tokenSource.Token);

                if (!tokenSource.Token.IsCancellationRequested && alteredTitle != null)
                {
                    DemoModeTitle = MediaContext.CreateLogo(alteredTitle);
                }
            }

            if (MediaContext.Titles.FirstOrDefault(t => t.IsSelected) is ImageViewModel title)
            {
                var alteredTitle = await MediaFormatterService.FormatTitle(title.BaseModel, tokenSource.Token);

                if (!tokenSource.Token.IsCancellationRequested && alteredTitle != null)
                {
                    DemoModeTitle = MediaContext.CreateTitle(alteredTitle);
                }
            }

            if (!tokenSource.Token.IsCancellationRequested)
            {
                DemoMediaChanged?.Invoke(this, this);

                IsFormattingTitle = false;

                _formatTitleTaskCompletionSource.SetResult();
            }
        }

        OnPropertyChanged(nameof(IsFound));
        OnPropertyChanged(nameof(NotIsFoundOrIsHover));
    }

    /// <summary>
    /// Updates the demo mode heroes by formatting all selected hero images and videos.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateDemoModeHeros()
    {
        if (MediaContext != null)
        {
            _formatHerosTaskCompletionSource = new TaskCompletionSource();

            if (_herosCancellationTokenSource != null)
            {
                await _herosCancellationTokenSource.CancelAsync();
            }

            _herosCancellationTokenSource = new CancellationTokenSource();

            var tokenSource = _herosCancellationTokenSource;

            IsFormattingHeros = true;

            DemoModeHeros.Clear();

            foreach (var hero in MediaContext.Heros.Where(h => h.IsSelected).OfType<ImageViewModel>())
            {
                Image? image = await MediaFormatterService.FormatHero(hero.BaseModel, tokenSource.Token);

                if (image != null && !tokenSource.Token.IsCancellationRequested)
                {
                    DemoModeHeros.Add(MediaContext.CreateHero(image));
                }
            }

            foreach (var hero in MediaContext.Videos.Where(v => v.IsSelected && v.ApplyMediaType == MediaType.Hero).OfType<VideoViewModel>())
            {
                await hero.Download();

                var crop = await MediaFormatterService.SmartCropVideo(hero.BaseModel);

                if (crop != null)
                {
                    hero.Crop = new CropViewModel(crop);
                }

                Video? video = await MediaFormatterService.FormatVideo(hero.BaseModel, tokenSource.Token);

                if (video != null && !tokenSource.Token.IsCancellationRequested)
                {
                    DemoModeHeros.Add(MediaContext.CreateVideo(video));
                }
            }

            if (!tokenSource.Token.IsCancellationRequested)
            {
                DemoMediaChanged?.Invoke(this, this);

                IsFormattingHeros = false;

                _formatHerosTaskCompletionSource.SetResult();
            }
        }

        OnPropertyChanged(nameof(IsFound));
        OnPropertyChanged(nameof(NotIsFoundOrIsHover));
    }

    /// <summary>
    /// Updates the demo mode slides by formatting all selected slide images and videos.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateDemoModeSlides()
    {
        if (MediaContext != null)
        {
            _formatSlidesTaskCompletionSource = new TaskCompletionSource();

            if (_slidesCancellationTokenSource != null)
            {
                await _slidesCancellationTokenSource.CancelAsync();
            }

            _slidesCancellationTokenSource = new CancellationTokenSource();

            var tokenSource = _slidesCancellationTokenSource;

            IsFormattingSlides = true;

            DemoModeSlides.Clear();

            foreach (ImageViewModel slide in MediaContext.Slides.Where(s => s.IsSelected).OfType<ImageViewModel>())
            {
                Image? image = await MediaFormatterService.FormatSlide(slide.BaseModel, tokenSource.Token);

                if (image != null && !tokenSource.Token.IsCancellationRequested)
                {
                    DemoModeSlides.Add(MediaContext.CreateSlide(image));
                }
            }

            foreach (var slide in MediaContext.Videos.Where(v => v.IsSelected && v.ApplyMediaType == MediaType.Slide).OfType<VideoViewModel>())
            {
                await slide.Download();

                var crop = await MediaFormatterService.SmartCropVideo(slide.BaseModel);

                if (crop != null)
                {
                    slide.Crop = new CropViewModel(crop);
                }

                Video? video = await MediaFormatterService.FormatVideo(slide.BaseModel, tokenSource.Token);

                if (video != null && !tokenSource.Token.IsCancellationRequested)
                {
                    DemoModeSlides.Add(MediaContext.CreateVideo(video));
                }
            }

            if (!tokenSource.Token.IsCancellationRequested)
            {
                DemoMediaChanged?.Invoke(this, this);

                IsFormattingSlides = false;

                _formatSlidesTaskCompletionSource.SetResult();
            }
        }

        OnPropertyChanged(nameof(IsFound));
        OnPropertyChanged(nameof(NotIsFoundOrIsHover));
    }

    /// <summary>
    /// Updates the demo mode music by downloading and preparing selected music tracks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateDemoModeMusic()
    {
        if (MediaContext != null)
        {
            _formatMusicTaskCompletionSource = new TaskCompletionSource();

            DemoModeMusic.Clear();

            foreach (var music in MediaContext.Music.Where(m => m.IsSelected))
            {
                var newMusic = MediaContext.CreateMusic(music.BaseModel);

                await newMusic.Download();

                DemoModeMusic.Add(newMusic);
            }

            SelectedDemoModeMusic = DemoModeMusic.FirstOrDefault();

            _formatMusicTaskCompletionSource.SetResult();
        }

        OnPropertyChanged(nameof(IsFetchMusicOrHasMusic));
        OnPropertyChanged(nameof(IsFound));
        OnPropertyChanged(nameof(NotIsFoundOrIsHover));
    }

    /// <summary>
    /// Updates all demo mode media including icon, title, heroes, slides, and music.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateDemoModeMedia()
    {
        if (MediaContext != null)
        {
            List<Task> tasks =
            [
                UpdateDemoModeIcon(),
                UpdateDemoModeTitle(),
                UpdateDemoModeHeros(),
                UpdateDemoModeSlides(),
                UpdateDemoModeMusic()
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
    /// Cancels the current URL entry and import operation.
    /// </summary>
    [RelayCommand]
    public async Task CancleUrl()
    {
        if (_cancelUrlFetchCancellationTokenSource != null)
        {
            await _cancelUrlFetchCancellationTokenSource.CancelAsync();
        }

        IsImportingUrl = false;

        IsEnteringUrl = false;
    }

    /// <summary>
    /// Confirms the url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmUrl()
    {
        if (_cancelUrlFetchCancellationTokenSource != null)
        {
            await _cancelUrlFetchCancellationTokenSource.CancelAsync();
        }

        _cancelUrlFetchCancellationTokenSource = new CancellationTokenSource();

        switch (UrlType)
        {
            case "Icon":
                await ConfirmIconUrl(_cancelUrlFetchCancellationTokenSource.Token);
                break;
            case "Title":
                await ConfirmTitleUrl(_cancelUrlFetchCancellationTokenSource.Token);
                break;
            case "Logo":
                await ConfirmLogoUrl(_cancelUrlFetchCancellationTokenSource.Token);
                break;
            case "Hero":
                await ConfirmHeroUrl(_cancelUrlFetchCancellationTokenSource.Token);
                break;
            case "Slide":
                await ConfirmSlideUrl(_cancelUrlFetchCancellationTokenSource.Token);
                break;
            case "Music":
                await ConfirmMusicUrl(_cancelUrlFetchCancellationTokenSource.Token);
                break;
            case "Music Playlist":
                await ConfirmMusicPlaylistUrl(_cancelUrlFetchCancellationTokenSource.Token);
                break;
            case "Video":
                await ConfirmVideoUrl(_cancelUrlFetchCancellationTokenSource.Token);
                break;
        }

        IsClearItems = false;

        Url = "";
    }

    /// <summary>
    /// Shows the icon url entry screen
    /// </summary>
    [RelayCommand]
    public async Task EnterIconUrl()
    {
        Url = "";

        if (MediaContext != null)
        {
            IsEnteringUrl = true;

            UrlType = "Icon";
        }
    }

    /// <summary>
    /// Confirms the icon url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmIconUrl(CancellationToken token)
    {
        if (MediaContext != null && !string.IsNullOrWhiteSpace(Url))
        {
            IsImportingUrl = true;

            if (IsClearItems)
            {
                MediaContext.ClearIcons();
            }

            var image = new Image()
            {
                Url = Url,
                Source = SourceFlag.Paste
            };

            await Downloader.DownloadImage(image, token);

            if (!token.IsCancellationRequested)
            {
                await MediaContext.AddIcon(MediaContext.CreateIcon(image), true);

                IsImportingUrl = false;

                IsEnteringUrl = false;
            }
        }
    }

    /// <summary>
    /// Shows the title url entry screen
    /// </summary>
    [RelayCommand]
    public async Task EnterTitleUrl()
    {
        Url = "";

        if (MediaContext != null)
        {
            IsEnteringUrl = true;

            UrlType = "Title";
        }
    }

    /// <summary>
    /// Confirms the title url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmTitleUrl(CancellationToken token)
    {
        if (MediaContext != null && !string.IsNullOrWhiteSpace(Url))
        {
            IsImportingUrl = true;

            if (IsClearItems)
            {
                MediaContext.ClearTitles();
            }

            var image = new Image()
            {
                Url = Url,
                Source = SourceFlag.Paste
            };

            await Downloader.DownloadImage(image, token);

            if (!token.IsCancellationRequested)
            {
                await MediaContext.AddTitle(MediaContext.CreateTitle(image), true);

                IsImportingUrl = false;

                IsEnteringUrl = false;
            }
        }
    }

    /// <summary>
    /// Shows the logo url entry screen
    /// </summary>
    [RelayCommand]
    public async Task EnterLogoUrl()
    {
        Url = "";

        if (MediaContext != null)
        {
            IsEnteringUrl = true;

            UrlType = "Logo";
        }
    }

    /// <summary>
    /// Confirms the logo url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmLogoUrl(CancellationToken token)
    {
        if (MediaContext != null && !string.IsNullOrWhiteSpace(Url))
        {
            IsImportingUrl = true;

            if (IsClearItems)
            {
                MediaContext.ClearLogos();
            }

            var image = new Image()
            {
                Url = Url,
                Source = SourceFlag.Paste
            };

            await Downloader.DownloadImage(image, token);

            if (!token.IsCancellationRequested)
            {
                await MediaContext.AddLogo(MediaContext.CreateLogo(image), true);

                IsImportingUrl = false;

                IsEnteringUrl = false;
            }
        }
    }

    /// <summary>
    /// Shows the hero url entry screen
    /// </summary>
    [RelayCommand]
    public async Task EnterHeroUrl()
    {
        Url = "";

        if (MediaContext != null)
        {
            IsEnteringUrl = true;

            UrlType = "Hero";
        }
    }

    /// <summary>
    /// Confirms the hero url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmHeroUrl(CancellationToken token)
    {
        if (MediaContext != null && !string.IsNullOrWhiteSpace(Url))
        {
            IsImportingUrl = true;

            if (IsClearItems)
            {
                MediaContext.ClearHeros();
            }

            var image = new Image()
            {
                Url = Url,
                Source = SourceFlag.Paste
            };

            await Downloader.DownloadImage(image, token);

            if (!token.IsCancellationRequested)
            {
                await MediaContext.AddHero(MediaContext.CreateHero(image), true);

                IsImportingUrl = false;

                IsEnteringUrl = false;
            }
        }
    }

    /// <summary>
    /// Shows the slide url entry screen
    /// </summary>
    [RelayCommand]
    public async Task EnterSlideUrl()
    {
        Url = "";

        if (MediaContext != null)
        {
            IsEnteringUrl = true;

            UrlType = "Slide";
        }
    }

    /// <summary>
    /// Confirms the slide url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmSlideUrl(CancellationToken token)
    {
        if (MediaContext != null && !string.IsNullOrWhiteSpace(Url))
        {
            IsImportingUrl = true;

            if (IsClearItems)
            {
                MediaContext.ClearSlides();
            }

            var image = new Image()
            {
                Url = Url,
                Source = SourceFlag.Paste
            };

            await Downloader.DownloadImage(image, token);

            if (!token.IsCancellationRequested)
            {
                await MediaContext.AddSlide(MediaContext.CreateSlide(image), true);

                IsImportingUrl = false;

                IsEnteringUrl = false;
            }
        }
    }

    /// <summary>
    /// Shows the music url entry screen
    /// </summary>
    [RelayCommand]
    public async Task EnterMusicUrl()
    {
        Url = "";

        if (MediaContext != null)
        {
            IsEnteringUrl = true;

            UrlType = "Music";
        }
    }

    /// <summary>
    /// Confirms the music url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmMusicUrl(CancellationToken token)
    {
        if (MediaContext != null && !string.IsNullOrWhiteSpace(Url))
        {
            IsImportingUrl = true;

            if (IsClearItems)
            {
                MediaContext.ClearMusic();
            }

            var music = new Music()
            {
                Url = Url,
                Source = SourceFlag.Paste
            };

            await Downloader.DownloadMusicDetails(music, token);

            if (!token.IsCancellationRequested)
            {
                await MediaContext.AddMusic(MediaContext.CreateMusic(music), true);

                IsImportingUrl = false;

                IsEnteringUrl = false;
            }
        }
    }

    /// <summary>
    /// Shows the music url entry screen
    /// </summary>
    [RelayCommand]
    public async Task EnterMusicPlaylistUrl()
    {
        Url = "";

        if (MediaContext != null)
        {
            IsEnteringUrl = true;

            UrlType = "Music Playlist";
        }
    }

    /// <summary>
    /// Confirms the music url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmMusicPlaylistUrl(CancellationToken token)
    {
        if (MediaContext != null && !string.IsNullOrWhiteSpace(Url))
        {
            IsImportingUrl = true;

            if (IsClearItems)
            {
                MediaContext.ClearMusic();
            }

            var musics = await Downloader.DownloadPlaylistDetails(Url, token);

            if (!token.IsCancellationRequested)
            {
                foreach (var music in musics)
                {
                    await MediaContext.AddMusic(MediaContext.CreateMusic(music));
                }

                IsImportingUrl = false;

                IsEnteringUrl = false;
            }
        }
    }

    /// <summary>
    /// Shows the video url entry screen
    /// </summary>
    [RelayCommand]
    public async Task EnterVideoUrl()
    {
        Url = "";

        if (MediaContext != null)
        {
            IsEnteringUrl = true;

            UrlType = "Video";
        }
    }


    /// <summary>
    /// Confirms the video url entry screen
    /// </summary>
    [RelayCommand]
    public async Task ConfirmVideoUrl(CancellationToken token)
    {
        if (MediaContext != null && !string.IsNullOrWhiteSpace(Url))
        {
            IsEnteringUrl = false;

            if (IsClearItems)
            {
                MediaContext.ClearVideo();
            }

            var video = new Video()
            {
                Url = Url,
                Source = SourceFlag.Paste,
                ApplyMediaType = MediaType.Slide
            };

            await Downloader.DownloadVideoDetails(video, token);

            if (!token.IsCancellationRequested)
            {
                await MediaContext.AddVideo(MediaContext.CreateVideo(video), true);

                IsImportingUrl = false;

                IsEnteringUrl = false;
            }
        }
    }

    /// <summary>
    /// Applies the scraped media to the game's asset folder.
    /// </summary>
    public async Task Apply()
    {
        // Wait for scraping and all formatting tasks to complete before applying
        await _scrapTaskCompletionSource.Task;

        if (_formatIconTaskCompletionSource != null)
        {
            await _formatIconTaskCompletionSource.Task;
        }

        if (_formatTitleTaskCompletionSource != null)
        {
            await _formatTitleTaskCompletionSource.Task;
        }

        if (_formatHerosTaskCompletionSource != null)
        {
            await _formatHerosTaskCompletionSource.Task;
        }

        if (_formatSlidesTaskCompletionSource != null)
        {
            await _formatSlidesTaskCompletionSource.Task;
        }

        if (_formatMusicTaskCompletionSource != null)
        {
            await _formatMusicTaskCompletionSource.Task;
        }

        // Handle game file organization based on whether media was found
        if (IsFound)
        {
            // Move game back to main folder if it was previously in unfound folder
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
            // Move game to unfound folder if no media was found
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

        // Apply media assets to the output folder
        if (!string.IsNullOrEmpty(Configuration.ApplyAssetPath) &&
            !string.IsNullOrEmpty(Folder))
        {
            if (IsFound || Configuration.IsApplyUnfoundGames)
            {
                // Create the game's asset folder
                var path = FileService.CombinePath(Configuration.ApplyAssetPath, Folder, FileService.CleanFileName(Name));

                await FileService.CreateDirectory(path);

                // Apply icon image
                if (Configuration.IsApplyIcon && !string.IsNullOrWhiteSpace(Configuration.IconNameFormat) && DemoModeIcon != null && !string.IsNullOrWhiteSpace(DemoModeIcon.LocalPath))
                {
                    await FileService.CopyFile(DemoModeIcon.LocalPath, FileService.CombinePath(path, $"{Configuration.IconNameFormat}.png"));
                }

                // Apply title image
                if (Configuration.IsApplyTitle && !string.IsNullOrWhiteSpace(Configuration.TitleNameFormat) && DemoModeTitle != null && !string.IsNullOrWhiteSpace(DemoModeTitle.LocalPath))
                {
                    await FileService.CopyFile(DemoModeTitle.LocalPath, FileService.CombinePath(path, $"{Configuration.TitleNameFormat}.png"));
                }

                // Apply hero images (multiple, numbered)
                if (Configuration.IsApplyHeros && !string.IsNullOrWhiteSpace(Configuration.HeroNameFormat))
                {
                    // Delete existing hero files before applying new ones
                    await FileService.DeleteFiles(path, $"{Configuration.HeroNameFormat}.*");

                    for (int i = 0; i < DemoModeHeros.Count; i++)
                    {
                        var localPath = DemoModeHeros[i].LocalPath;

                        if (!string.IsNullOrWhiteSpace(localPath))
                        {
                            await FileService.CopyFile(localPath, FileService.CombinePath(path, $"{Configuration.HeroNameFormat}{i + 1}.{(DemoModeHeros[i] is ImageViewModel ? "png" : "mp4")}"));
                        }
                    }
                }

                // Apply slide images (multiple, numbered)
                if (Configuration.IsApplySlides && !string.IsNullOrWhiteSpace(Configuration.SlideNameFormat))
                {
                    // Delete existing slide files before applying new ones
                    await FileService.DeleteFiles(path, $"{Configuration.SlideNameFormat}.*");

                    for (int i = 0; i < DemoModeSlides.Count; i++)
                    {
                        var localPath = DemoModeSlides[i].LocalPath;

                        if (!string.IsNullOrWhiteSpace(localPath))
                        {
                            await FileService.CopyFile(localPath, FileService.CombinePath(path, $"{Configuration.SlideNameFormat}{i + 1}.{(DemoModeSlides[i] is ImageViewModel ? "png" : "mp4")}"));
                        }
                    }
                }

                // Apply music files (multiple, numbered)
                if (Configuration.IsApplyMusic)
                {
                    // Delete existing music files before applying new ones
                    await FileService.DeleteFiles(path, $"{Configuration.MusicNameFormat}.*");

                    for (int i = 0; i < DemoModeMusic.Count; i++)
                    {
                        var localPath = DemoModeMusic[i].LocalPath;

                        var name = string.IsNullOrWhiteSpace(Configuration.MusicNameFormat) ? DemoModeMusic[i].Title : $"{Configuration.MusicNameFormat}{i + 1}";

                        if (!string.IsNullOrWhiteSpace(localPath))
                        {
                            await FileService.CopyFile(localPath, FileService.CombinePath(path, $"{name}.mp3"));
                        }
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
        // Reset task completion source for new scrape operation
        _scrapTaskCompletionSource = new TaskCompletionSource();

        IsLoading = true;

        if (!string.IsNullOrWhiteSpace(Path) && !string.IsNullOrWhiteSpace(Platform))
        {
            // Fetch media from all configured scrapers (IGN, SteamGridDB, IGDB, YouTube)
            MediaContext mediaContext = await ScrapingService.GetMedia(Platform, Name);

            // Clean up event handlers from previous media context
            if (MediaContext != null)
            {
                DeInitializeMediaContext(MediaContext);
            }

            if (mediaContext != null)
            {
                // Create view model wrapper and wire up event handlers
                MediaContext = new MediaContextViewModel(mediaContext, ScrapingService, MediaFormatterService, UpscalerService, FileService, Downloader, Configuration);

                InitializeMediaContext(MediaContext);
            }
        }

        // Signal that scraping is complete (Format() waits on this)
        _scrapTaskCompletionSource.SetResult();
    }

    /// <summary>
    /// Formats media for the game by calculating smart crop regions.
    /// Waits for scraping to complete before formatting.
    /// </summary>
    public async Task Format()
    {
        // Wait for scraping to complete before formatting
        await _scrapTaskCompletionSource.Task;

        // Clear existing demo mode media before formatting
        DemoModeHeros.Clear();
        DemoModeSlides.Clear();

        if (MediaContext != null)
        {
            // Calculate smart crop regions for each image type in parallel
            // This determines optimal crop areas based on content analysis
            var smartCropTasks = new List<Task>();

            smartCropTasks.AddRange(MediaContext.Icons.Where(i => i.IsSelected).Select(i => OnCropMedia(i, MediaType.Icon)));

            smartCropTasks.AddRange(MediaContext.Logos.Where(l => l.IsSelected).Select(l => OnCropMedia(l, MediaType.Logo)));

            smartCropTasks.AddRange(MediaContext.Titles.Where(t => t.IsSelected).Select(t => OnCropMedia(t, MediaType.Title)));

            smartCropTasks.AddRange(MediaContext.Heros.Where(h => h.IsSelected).OfType<ImageViewModel>().Select(h => OnCropMedia(h, MediaType.Hero)));

            smartCropTasks.AddRange(MediaContext.Slides.Where(s => s.IsSelected).OfType<ImageViewModel>().Select(s => OnCropMedia(s, MediaType.Hero)));

            await Task.WhenAll(smartCropTasks);

            // Generate demo mode preview with formatted media
            await UpdateDemoModeMedia();
        }

        IsLoading = false;
    }

    /// <summary>
    /// Gets the name of the game without file extension.
    /// </summary>
    public string Name => FileService?.GetFileNameWithoutExtension(Path ?? "") ?? "";

    /// <summary>
    /// Gets the formatted name of the game without file extension.
    /// </summary>
    public string FormattedName => CleanName(Name);


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
    /// Gets a value indicating whether the game has been found with all required media.
    /// </summary>
    public bool IsFound
    {
        get
        {
            if (MediaContext != null)
            {
                // Check if all required media types have been found based on configuration
                // Each check is only performed if that media type is marked as required
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

                if (Configuration.IsUnfoundMediaIfNoMusic)
                {
                    isfound &= MediaContext.Music.Any();
                }

                if (Configuration.IsUnfoundMediaIfNoVideos)
                {
                    isfound &= MediaContext.Videos.Any();
                }

                return isfound;
            }

            return true;
        }
    }

    public bool IsFetchMusicOrHasMusic => Configuration.ScraperConfigurations.Any(s => s.IsFetchMusic) || DemoModeMusic.Count > 0;

    public bool NotIsFoundOrIsHover => !IsFound || IsHover;

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
