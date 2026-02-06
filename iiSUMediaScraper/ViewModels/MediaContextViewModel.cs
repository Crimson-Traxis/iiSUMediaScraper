using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ObservableModels;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using System;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for a collection of media items associated with a game.
/// </summary>
public partial class MediaContextViewModel : ObservableMediaContext
{
    /// <summary>
    /// Raised when removal is requested for the media context.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Gets or sets the collection of icon images.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ImageViewModel> icons;

    /// <summary>
    /// Gets or sets the collection of logo images.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ImageViewModel> logos;

    /// <summary>
    /// Gets or sets the collection of title images.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ImageViewModel> titles;

    /// <summary>
    /// Gets or sets the collection of hero media items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ImageViewModel> heros;

    /// <summary>
    /// Gets or sets the collection of slide media items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ImageViewModel> slides;

    /// <summary>
    /// Gets or sets the collection of music media items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MusicViewModel> music;

    /// <summary>
    /// Gets or sets the collection of video media items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<VideoViewModel> videos;

    /// <summary>
    /// Gets or sets the configuration view model.
    /// </summary>
    [ObservableProperty]
    private ConfigurationViewModel configuration;

    /// <summary>
    /// Raised when a preview is requested for an image.
    /// </summary>
    public event EventHandler<ImageViewModel>? PreviewRequested;

    /// <summary>
    /// Raised when stopping a preview is requested for an image.
    /// </summary>
    public event EventHandler<ImageViewModel>? StopPreviewRequested;

    /// <summary>
    /// Raised when editing is requested for an image.
    /// </summary>
    public event EventHandler<ImageViewModel>? EditRequested;

    /// <summary>
    /// Raised when the media selection changes.
    /// </summary>
    public event EventHandler<MediaViewModel>? SelectionChanged;

    /// <summary>
    /// Raised when the icon selection changes.
    /// </summary>
    public event EventHandler<ImageViewModel>? IconSelectionChanged;

    /// <summary>
    /// Raised when the title selection changes.
    /// </summary>
    public event EventHandler<ImageViewModel>? TitleSelectionChanged;

    /// <summary>
    /// Raised when the logo selection changes.
    /// </summary>
    public event EventHandler<ImageViewModel>? LogoSelectionChanged;

    /// <summary>
    /// Raised when the hero selection changes.
    /// </summary>
    public event EventHandler<ImageViewModel>? HeroSelectionChanged;

    /// <summary>
    /// Raised when the slide selection changes.
    /// </summary>
    public event EventHandler<ImageViewModel>? SlideSelectionChanged;

    /// <summary>
    /// Raised when the music selection changes.
    /// </summary>
    public event EventHandler<MusicViewModel>? MusicSelectionChanged;

    /// <summary>
    /// Raised when the video selection changes.
    /// </summary>
    public event EventHandler<VideoViewModel>? VideoSelectionChanged;

    /// <summary>
    /// Raised when the music requests to be played.
    /// </summary>
    public event EventHandler? MusicPlayRequested;

    /// <summary>
    /// Raised when the video requests to be played.
    /// </summary>
    public event EventHandler? VideoPlayRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContextViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying media context model.</param>
    /// <param name="scrapingService">The scraping service.</param>
    /// <param name="mediaFormatterService">The media formatter service.</param>
    /// <param name="upscalerService">The upscaler service.</param>
    /// <param name="fileService">The file service.</param>
    /// <param name="configuration">The configuration view model.</param>
    public MediaContextViewModel(MediaContext baseModel,
                                 IScrapingService scrapingService,
                                 IMediaFormatterService mediaFormatterService,
                                 IUpscalerService upscalerService,
                                 IFileService fileService,
                                 IDownloader downloader,
                                 ConfigurationViewModel configuration) : base(baseModel)
    {
        ScrapingService = scrapingService;

        MediaFormatterService = mediaFormatterService;

        UpscalerService = upscalerService;

        FileService = fileService;

        Downloader = downloader;

        Configuration = configuration;

        icons = [];
        logos = [];
        titles = [];
        heros = [];
        slides = [];
        music = [];
        videos = [];

        RegisterBaseModelObservableCollection(
            nameof(Icons),
            baseModel.Icons,
            icons,
            CreateIcon,
            InitializeIcon);

        RegisterBaseModelObservableCollection(
            nameof(Logos),
            baseModel.Logos,
            logos,
            CreateLogo,
            InitializeLogo);

        RegisterBaseModelObservableCollection(
            nameof(Titles),
            baseModel.Titles,
            titles,
            CreateTitle,
            InitializeTitle);

        RegisterBaseModelObservableCollection(
            nameof(Heros),
            baseModel.Heros,
            heros,
            CreateHero,
            InitializeHero);

        RegisterBaseModelObservableCollection(
            nameof(Slides),
            baseModel.Slides,
            slides,
            CreateSlide,
            InitializeSlide);

        RegisterBaseModelObservableCollection(
            nameof(Music),
            baseModel.Music,
            music,
            CreateMusic,
            InitializeMusic);

        RegisterBaseModelObservableCollection(
            nameof(Videos),
            baseModel.Videos,
            videos,
            CreateVideo,
            InitializeVideo);

        // Select previous media if available, otherwise use default selection logic
        var previousIcon = icons.FirstOrDefault(i => i.Source == SourceFlag.Previous);
        var previousTitle = titles.FirstOrDefault(t => t.Source == SourceFlag.Previous);
        var previousLogo = logos.FirstOrDefault(l => l.Source == SourceFlag.Previous);
        var previousHero = heros.FirstOrDefault(h => h.Source == SourceFlag.Previous);
        var previousMusic = music.FirstOrDefault(m => m.Source == SourceFlag.Previous);
        var previousSlides = slides.Where(s => s.Source == SourceFlag.Previous).ToList();
        var previousVideos = videos.Where(v => v.Source == SourceFlag.Previous).ToList();

        if (previousIcon != null)
        {
            previousIcon.IsSelected = true;
        }
        else if (icons.Any())
        {
            icons.First().IsSelected = true;
        }

        if (previousTitle != null)
        {
            previousTitle.IsSelected = true;
        }
        else if (previousLogo != null && (Configuration.IsPrioritizeLogoAsTitle || Configuration.IsUseLogoAsTitle))
        {
            previousLogo.IsSelected = true;
        }
        else if ((Configuration.IsPrioritizeLogoAsTitle || Configuration.IsUseLogoAsTitle) && logos.Any())
        {
            logos.First().IsSelected = true;
        }
        else if (titles.Any() && !Configuration.IsUseLogoAsTitle)
        {
            titles.First().IsSelected = true;
        }

        if (previousHero != null)
        {
            previousHero.IsSelected = true;
        }
        else if (heros.Any())
        {
            heros.First().IsSelected = true;
        }

        if (previousMusic != null)
        {
            previousMusic.IsSelected = true;
        }
        else if (music.Any())
        {
            music.First().IsSelected = true;
        }

        // For slides, select all previous slides if any exist, otherwise select all slides
        if (previousSlides.Any())
        {
            foreach (var slide in previousSlides)
            {
                slide.IsSelected = true;
            }
        }
        else
        {
            foreach (MediaViewModel slide in slides)
            {
                slide.IsSelected = true;
            }
        }

        // Select previous videos
        foreach (var video in previousVideos)
        {
            video.IsSelected = true;
        }
    }

    /// <summary>
    /// Handles the preview requested event from an image.
    /// </summary>
    /// <param name="sender">The image requesting preview.</param>
    /// <param name="e">The image view model.</param>
    private void OnPreviewRequested(object? sender, ImageViewModel e)
    {
        PreviewRequested?.Invoke(this, e);
    }

    /// <summary>
    /// Handles the stop preview requested event from an image.
    /// </summary>
    /// <param name="sender">The image requesting to stop preview.</param>
    /// <param name="e">The image view model.</param>
    private void OnStopPreviewRequested(object? sender, ImageViewModel e)
    {
        StopPreviewRequested?.Invoke(this, e);
    }

    /// <summary>
    /// Handles the edit requested event from an image.
    /// </summary>
    /// <param name="sender">The image requesting edit.</param>
    /// <param name="e">The image view model.</param>
    private void OnEditRequested(object? sender, ImageViewModel e)
    {
        EditRequested?.Invoke(this, e);
    }

    /// <summary>
    /// Handles the selection changed event for an icon.
    /// </summary>
    /// <param name="sender">The icon whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void Icon_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel imageViewModel)
        {
            OnIconSelectionChanged(imageViewModel); 
            
            SelectionChanged?.Invoke(this, imageViewModel);
        }
    }

    /// <summary>
    /// Handles the selection changed event for a logo.
    /// </summary>
    /// <param name="sender">The logo whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void Logo_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel imageViewModel)
        {
            OnLogoSelectionChanged(imageViewModel);

            SelectionChanged?.Invoke(this, imageViewModel);
        }
    }

    /// <summary>
    /// Handles the selection changed event for a title.
    /// </summary>
    /// <param name="sender">The title whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void Title_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel imageViewModel)
        {
            OnTitleSelectionChanged(imageViewModel);

            SelectionChanged?.Invoke(this, imageViewModel);
        }
    }

    /// <summary>
    /// Handles the selection changed event for a hero.
    /// </summary>
    /// <param name="sender">The hero whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void Hero_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel imageViewModel)
        {
            OnHeroSelectionChanged(imageViewModel);

            SelectionChanged?.Invoke(this, imageViewModel);
        }
    }

    /// <summary>
    /// Handles the selection changed event for a slide.
    /// </summary>
    /// <param name="sender">The slide whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void Slide_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel imageViewModel)
        {
            OnSlideSelectionChanged(imageViewModel);

            SelectionChanged?.Invoke(this, imageViewModel);

        }
    }

    /// <summary>
    /// Handles the selection changed event for a music item.
    /// </summary>
    /// <param name="sender">The music item whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void Music_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is MusicViewModel musicViewModel)
        {
            OnMusicSelectionChanged(musicViewModel);

            SelectionChanged?.Invoke(this, musicViewModel);
        }
    }

    /// <summary>
    /// Handles the selection changed event for a video item.
    /// </summary>
    /// <param name="sender">The video item whose selection changed.</param>
    /// <param name="e">The event arguments.</param>
    private void Video_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is VideoViewModel videoViewModel)
        {
            OnVideoSelectionChanged(videoViewModel);

            SelectionChanged?.Invoke(this, videoViewModel);
        }
    }

    /// <summary>
    /// Handles the remove requested event for an icon.
    /// </summary>
    /// <param name="sender">The icon requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnIconRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel item)
        {
            RemoveIcon(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a logo.
    /// </summary>
    /// <param name="sender">The logo requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnLogoRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel item)
        {
            RemoveLogo(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a title.
    /// </summary>
    /// <param name="sender">The title requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnTitleRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel item)
        {
            RemoveTitle(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a hero.
    /// </summary>
    /// <param name="sender">The hero requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnHeroRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel item)
        {
            RemoveHero(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a slide.
    /// </summary>
    /// <param name="sender">The slide requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnSlideRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel item)
        {
            RemoveSlide(item);
        }
    }

    /// <summary>
    /// Handles the remove requested event for a music item.
    /// </summary>
    /// <param name="sender">The music item requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnMusicRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is MusicViewModel item)
        {
            RemoveMusic(item);
        }
    }

    /// <summary>
    /// Handles the play requested event for a music item.
    /// </summary>
    /// <param name="sender">The music item requesting playback.</param>
    /// <param name="e">The event arguments.</param>
    private void OnMusicPlayRequested(object? sender, EventArgs e)
    {
        MusicPlayRequested?.Invoke(sender, e);
    }

    /// <summary>
    /// Handles the remove requested event for a video item.
    /// </summary>
    /// <param name="sender">The video item requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnVideoRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is VideoViewModel item)
        {
            RemoveVideo(item);
        }
    }

    /// <summary>
    /// Handles the play requested event for a video item.
    /// </summary>
    /// <param name="sender">The video item requesting playback.</param>
    /// <param name="e">The event arguments.</param>
    private void OnVideoPlayRequested(object? sender, EventArgs e)
    {
        VideoPlayRequested?.Invoke(sender, e);
    }

    /// <summary>
    /// Processes icon selection change and ensures only one icon is selected.
    /// </summary>
    /// <param name="imageViewModel">The icon whose selection changed.</param>
    private void OnIconSelectionChanged(ImageViewModel imageViewModel)
    {
        if (imageViewModel.IsSelected)
        {
            foreach (MediaViewModel? icon in Icons.Except([imageViewModel]))
            {
                icon.IsSelected = false;
            }
        }

        IconSelectionChanged?.Invoke(this, imageViewModel);
    }

    /// <summary>
    /// Processes title selection change, deselecting other titles and logos.
    /// </summary>
    /// <param name="imageViewModel">The title whose selection changed.</param>
    private void OnTitleSelectionChanged(ImageViewModel imageViewModel)
    {
        if (imageViewModel.IsSelected)
        {
            foreach (MediaViewModel? title in Titles.Except([imageViewModel]))
            {
                title.IsSelected = false;
            }

            foreach (ImageViewModel logo in Logos)
            {
                logo.IsSelected = false;
            }
        }

        TitleSelectionChanged?.Invoke(this, imageViewModel);
    }

    /// <summary>
    /// Processes logo selection change, deselecting titles and other logos.
    /// </summary>
    /// <param name="imageViewModel">The logo whose selection changed.</param>
    private void OnLogoSelectionChanged(ImageViewModel imageViewModel)
    {
        if (imageViewModel.IsSelected)
        {
            foreach (ImageViewModel title in Titles)
            {
                title.IsSelected = false;
            }

            foreach (MediaViewModel? logo in Logos.Except([imageViewModel]))
            {
                logo.IsSelected = false;
            }
        }

        LogoSelectionChanged?.Invoke(this, imageViewModel);
    }

    /// <summary>
    /// Processes hero selection change and raises the selection changed event.
    /// </summary>
    /// <param name="imageViewModel">The hero whose selection changed.</param>
    private void OnHeroSelectionChanged(ImageViewModel imageViewModel)
    {
        HeroSelectionChanged?.Invoke(this, imageViewModel);
    }

    /// <summary>
    /// Processes slide selection change and raises the selection changed event.
    /// </summary>
    /// <param name="imageViewModel">The slide whose selection changed.</param>
    private void OnSlideSelectionChanged(ImageViewModel imageViewModel)
    {
        SlideSelectionChanged?.Invoke(this, imageViewModel);
    }

    /// <summary>
    /// Processes music selection change and raises the selection changed event.
    /// </summary>
    /// <param name="musicViewModel">The music item whose selection changed.</param>
    private void OnMusicSelectionChanged(MusicViewModel musicViewModel)
    {
        MusicSelectionChanged?.Invoke(this, musicViewModel);
    }

    /// <summary>
    /// Processes video selection change and raises the selection changed event.
    /// </summary>
    /// <param name="videoViewModel">The video item whose selection changed.</param>
    private void OnVideoSelectionChanged(VideoViewModel videoViewModel)
    {
        VideoSelectionChanged?.Invoke(this, videoViewModel);
    }

    /// <summary>
    /// Reads local data for a media item, setting the local path from URL if needed.
    /// </summary>
    /// <param name="mediaViewModel">The media view model to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task ReadLocalData(MediaViewModel mediaViewModel)
    {
        if (!string.IsNullOrEmpty(mediaViewModel.Url) && string.IsNullOrEmpty(mediaViewModel.LocalPath))
        {
            mediaViewModel.LocalPath = mediaViewModel.Url;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Processes a newly added image, updating its size and calculating crop if needed.
    /// </summary>
    /// <param name="imageViewModel">The image view model that was added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnImageAdded(ImageViewModel imageViewModel)
    {
        if (imageViewModel.Width == 0 || imageViewModel.Height == 0)
        {
            await MediaFormatterService.UpdateImageSize(imageViewModel.BaseModel);
        }

        if (imageViewModel.Crop == null)
        {
            Crop? crop = null;

            switch (imageViewModel.MediaType)
            {
                case MediaType.Icon:
                    crop = await MediaFormatterService.SmartCropIcon(imageViewModel.BaseModel);
                    break;
                case MediaType.Logo:
                    crop = await MediaFormatterService.SmartCropLogo(imageViewModel.BaseModel);
                    break;
                case MediaType.Title:
                    crop = await MediaFormatterService.SmartCropTitle(imageViewModel.BaseModel);
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
        }
    }

    /// <summary>
    /// Initializes event handlers for an icon image.
    /// </summary>
    /// <param name="item">The icon image to initialize.</param>
    protected void InitializeIcon(ImageViewModel item)
    {
        item.RemoveRequested += OnIconRemoveRequested;
        item.IsSelectedChanged += Icon_IsSelectedChanged;
        item.PreviewRequested += OnPreviewRequested;
        item.StopPreviewRequested += OnStopPreviewRequested;
        item.EditRequested += OnEditRequested;
    }

    /// <summary>
    /// Removes event handlers from an icon image.
    /// </summary>
    /// <param name="item">The icon image to de-initialize.</param>
    protected void DeInitializeIcon(ImageViewModel item)
    {
        item.RemoveRequested -= OnIconRemoveRequested;
        item.IsSelectedChanged -= Icon_IsSelectedChanged;
        item.PreviewRequested -= OnPreviewRequested;
        item.StopPreviewRequested -= OnStopPreviewRequested;
        item.EditRequested -= OnEditRequested;
    }

    /// <summary>
    /// Initializes event handlers for a logo image.
    /// </summary>
    /// <param name="item">The logo image to initialize.</param>
    protected void InitializeLogo(ImageViewModel item)
    {
        item.RemoveRequested += OnLogoRemoveRequested;
        item.IsSelectedChanged += Logo_IsSelectedChanged;
        item.PreviewRequested += OnPreviewRequested;
        item.StopPreviewRequested += OnStopPreviewRequested;
        item.EditRequested += OnEditRequested;
    }

    /// <summary>
    /// Removes event handlers from a logo image.
    /// </summary>
    /// <param name="item">The logo image to de-initialize.</param>
    protected void DeInitializeLogo(ImageViewModel item)
    {
        item.RemoveRequested -= OnLogoRemoveRequested;
        item.IsSelectedChanged -= Logo_IsSelectedChanged;
        item.PreviewRequested -= OnPreviewRequested;
        item.StopPreviewRequested -= OnStopPreviewRequested;
        item.EditRequested -= OnEditRequested;
    }

    /// <summary>
    /// Initializes event handlers for a title image.
    /// </summary>
    /// <param name="item">The title image to initialize.</param>
    protected void InitializeTitle(ImageViewModel item)
    {
        item.RemoveRequested += OnTitleRemoveRequested;
        item.IsSelectedChanged += Title_IsSelectedChanged;
        item.PreviewRequested += OnPreviewRequested;
        item.StopPreviewRequested += OnStopPreviewRequested;
        item.EditRequested += OnEditRequested;
    }

    /// <summary>
    /// Removes event handlers from a title image.
    /// </summary>
    /// <param name="item">The title image to de-initialize.</param>
    protected void DeInitializeTitle(ImageViewModel item)
    {
        item.RemoveRequested -= OnTitleRemoveRequested;
        item.IsSelectedChanged -= Title_IsSelectedChanged;
        item.PreviewRequested -= OnPreviewRequested;
        item.StopPreviewRequested -= OnStopPreviewRequested;
        item.EditRequested -= OnEditRequested;
    }

    /// <summary>
    /// Initializes event handlers for a hero image.
    /// </summary>
    /// <param name="item">The hero image to initialize.</param>
    protected void InitializeHero(ImageViewModel item)
    {
        item.RemoveRequested += OnHeroRemoveRequested;
        item.IsSelectedChanged += Hero_IsSelectedChanged;

        item.PreviewRequested += OnPreviewRequested;
        item.StopPreviewRequested += OnStopPreviewRequested;
        item.EditRequested += OnEditRequested;
    }

    /// <summary>
    /// Removes event handlers from a hero image.
    /// </summary>
    /// <param name="item">The hero image to de-initialize.</param>
    protected void DeInitializeHero(ImageViewModel item)
    {
        item.RemoveRequested -= OnHeroRemoveRequested;
        item.IsSelectedChanged -= Hero_IsSelectedChanged;

        item.PreviewRequested -= OnPreviewRequested;
        item.StopPreviewRequested -= OnStopPreviewRequested;
        item.EditRequested -= OnEditRequested;
    }

    /// <summary>
    /// Initializes event handlers for a slide image.
    /// </summary>
    /// <param name="item">The slide image to initialize.</param>
    protected void InitializeSlide(ImageViewModel item)
    {
        item.RemoveRequested += OnSlideRemoveRequested;
        item.IsSelectedChanged += Slide_IsSelectedChanged;

        item.PreviewRequested += OnPreviewRequested;
        item.StopPreviewRequested += OnStopPreviewRequested;
        item.EditRequested += OnEditRequested;
    }

    /// <summary>
    /// Removes event handlers from a slide image.
    /// </summary>
    /// <param name="item">The slide image to de-initialize.</param>
    protected void DeInitializeSlide(ImageViewModel item)
    {
        item.RemoveRequested -= OnSlideRemoveRequested;
        item.IsSelectedChanged -= Slide_IsSelectedChanged;

        item.PreviewRequested -= OnPreviewRequested;
        item.StopPreviewRequested -= OnStopPreviewRequested;
        item.EditRequested -= OnEditRequested;
    }

    /// <summary>
    /// Initializes event handlers for a music item.
    /// </summary>
    /// <param name="item">The music item to initialize.</param>
    protected void InitializeMusic(MusicViewModel item)
    {
        item.RemoveRequested += OnMusicRemoveRequested;
        item.PlayRequested += OnMusicPlayRequested;
        item.IsSelectedChanged += Music_IsSelectedChanged;
    }

    /// <summary>
    /// Removes event handlers from a music item.
    /// </summary>
    /// <param name="item">The music item to de-initialize.</param>
    protected void DeInitializeMusic(MusicViewModel item)
    {
        item.RemoveRequested -= OnMusicRemoveRequested;
        item.PlayRequested -= OnMusicPlayRequested;
        item.IsSelectedChanged -= Music_IsSelectedChanged;
    }

    /// <summary>
    /// Initializes event handlers for a video item.
    /// </summary>
    /// <param name="item">The video item to initialize.</param>
    protected void InitializeVideo(VideoViewModel item)
    {
        item.RemoveRequested += OnVideoRemoveRequested;
        item.PlayRequested += OnVideoPlayRequested;
        item.IsSelectedChanged += Video_IsSelectedChanged;
    }

    /// <summary>
    /// Removes event handlers from a video item.
    /// </summary>
    /// <param name="item">The video item to de-initialize.</param>
    protected void DeInitializeVideo(VideoViewModel item)
    {
        item.RemoveRequested -= OnVideoRemoveRequested;
        item.PlayRequested -= OnVideoPlayRequested;
        item.IsSelectedChanged -= Video_IsSelectedChanged;
    }

    /// <summary>
    /// Creates an icon image view model from a base image model.
    /// </summary>
    /// <param name="baseModel">The underlying image model.</param>
    /// <returns>A new icon image view model.</returns>
    public ImageViewModel CreateIcon(Image baseModel)
    {
        return new ImageViewModel(baseModel, MediaType.Icon, MediaFormatterService, UpscalerService, Downloader, Configuration);
    }

    /// <summary>
    /// Creates a logo image view model from a base image model.
    /// </summary>
    /// <param name="baseModel">The underlying image model.</param>
    /// <returns>A new logo image view model.</returns>
    public ImageViewModel CreateLogo(Image baseModel)
    {
        return new ImageViewModel(baseModel, MediaType.Logo, MediaFormatterService, UpscalerService, Downloader, Configuration);
    }

    /// <summary>
    /// Creates a title image view model from a base image model.
    /// </summary>
    /// <param name="baseModel">The underlying image model.</param>
    /// <returns>A new title image view model.</returns>
    public ImageViewModel CreateTitle(Image baseModel)
    {
        return new ImageViewModel(baseModel, MediaType.Title, MediaFormatterService, UpscalerService, Downloader, Configuration);
    }

    /// <summary>
    /// Creates a hero image view model from a base image model.
    /// </summary>
    /// <param name="baseModel">The underlying image model.</param>
    /// <returns>A new hero image view model.</returns>
    public ImageViewModel CreateHero(Image baseModel)
    {
        return new ImageViewModel(baseModel, MediaType.Hero, MediaFormatterService, UpscalerService, Downloader, Configuration);
    }

    /// <summary>
    /// Creates a slide image view model from a base image model.
    /// </summary>
    /// <param name="baseModel">The underlying image model.</param>
    /// <returns>A new slide image view model.</returns>
    public ImageViewModel CreateSlide(Image baseModel)
    {
        return new ImageViewModel(baseModel, MediaType.Slide, MediaFormatterService, UpscalerService, Downloader, Configuration);
    }

    /// <summary>
    /// Creates a music view model from a base music model.
    /// </summary>
    /// <param name="baseModel">The underlying music model.</param>
    /// <returns>A new music view model.</returns>
    public MusicViewModel CreateMusic(Music baseModel)
    {
        return new MusicViewModel(baseModel, MediaType.Music, Downloader, Configuration);
    }

    /// <summary>
    /// Creates a video view model from a base video model.
    /// </summary>
    /// <param name="baseModel">The underlying video model.</param>
    /// <returns>A new video view model.</returns>
    public VideoViewModel CreateVideo(Video baseModel)
    {
        return new VideoViewModel(baseModel, MediaType.Video, Downloader, Configuration);
    }

    /// <summary>
    /// Creates a new icon and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public async Task CreateNewIcon()
    {
        await InsertIcon(0, CreateIcon(new Image()));
    }

    /// <summary>
    /// Creates a new logo and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public async Task CreateNewLogo()
    {
        await InsertLogo(0, CreateLogo(new Image()));
    }

    /// <summary>
    /// Creates a new title and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public async Task CreateNewTitle()
    {
        await InsertTitle(0, CreateTitle(new Image()));
    }

    /// <summary>
    /// Creates a new hero and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public async Task CreateNewHero()
    {
        await InsertHero(0, CreateHero(new Image()));
    }

    /// <summary>
    /// Creates a new slide and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public async Task CreateNewSlide()
    {
        await InsertSlide(0, CreateSlide(new Image()));
    }

    /// <summary>
    /// Adds an icon to the collection.
    /// </summary>
    /// <param name="item">The icon to add.</param>
    /// <param name="isSelected">Whether the icon should be selected after adding.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddIcon(ImageViewModel item, bool isSelected = false)
    {
        InitializeIcon(item);

        Icons.Add(item);

        item.MediaType = MediaType.Icon;

        await OnImageAdded(item);

        if (isSelected)
        {
            item.IsSelected = true;
            OnIconSelectionChanged(item);
        }
    }

    /// <summary>
    /// Inserts an icon at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The icon to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertIcon(int index, ImageViewModel item)
    {
        InitializeIcon(item);

        Icons.Insert(index, item);

        item.MediaType = MediaType.Icon;

        await OnImageAdded(item);
    }

    /// <summary>
    /// Removes an icon from the collection.
    /// </summary>
    /// <param name="item">The icon to remove.</param>
    public void RemoveIcon(ImageViewModel item)
    {
        DeInitializeIcon(item);
        Icons.Remove(item);
    }

    /// <summary>
    /// Clears all icons from the collection.
    /// </summary>
    public void ClearIcons()
    {
        foreach (ImageViewModel? item in Icons.ToList())
        {
            RemoveIcon(item);
        }
    }

    /// <summary>
    /// Adds a logo to the collection.
    /// </summary>
    /// <param name="item">The logo to add.</param>
    /// <param name="isSelected">Whether the logo should be selected after adding.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddLogo(ImageViewModel item, bool isSelected = false)
    {
        InitializeLogo(item);

        Logos.Add(item);

        item.MediaType = MediaType.Logo;

        await OnImageAdded(item);

        if (isSelected)
        {
            item.IsSelected = true;
            OnLogoSelectionChanged(item);
        }
    }

    /// <summary>
    /// Inserts a logo at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The logo to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertLogo(int index, ImageViewModel item)
    {
        InitializeLogo(item);

        Logos.Insert(index, item);

        item.MediaType = MediaType.Logo;

        await OnImageAdded(item);
    }

    /// <summary>
    /// Removes a logo from the collection.
    /// </summary>
    /// <param name="item">The logo to remove.</param>
    public void RemoveLogo(ImageViewModel item)
    {
        DeInitializeLogo(item);
        Logos.Remove(item);
    }

    /// <summary>
    /// Clears all logos from the collection.
    /// </summary>
    public void ClearLogos()
    {
        foreach (ImageViewModel? item in Logos.ToList())
        {
            RemoveLogo(item);
        }
    }

    /// <summary>
    /// Adds a title to the collection.
    /// </summary>
    /// <param name="item">The title to add.</param>
    /// <param name="isSelected">Whether the title should be selected after adding.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddTitle(ImageViewModel item, bool isSelected = false)
    {
        InitializeTitle(item);

        Titles.Add(item);

        item.MediaType = MediaType.Title;

        await OnImageAdded(item);

        if (isSelected)
        {
            item.IsSelected = true;
            OnTitleSelectionChanged(item);
        }
    }

    /// <summary>
    /// Inserts a title at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The title to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertTitle(int index, ImageViewModel item)
    {
        InitializeTitle(item);

        Titles.Insert(index, item);

        item.MediaType = MediaType.Title;

        await OnImageAdded(item);
    }

    /// <summary>
    /// Removes a title from the collection.
    /// </summary>
    /// <param name="item">The title to remove.</param>
    public void RemoveTitle(ImageViewModel item)
    {
        DeInitializeTitle(item);
        Titles.Remove(item);
    }

    /// <summary>
    /// Clears all titles from the collection.
    /// </summary>
    public void ClearTitles()
    {
        foreach (ImageViewModel? item in Titles.ToList())
        {
            RemoveTitle(item);
        }
    }

    /// <summary>
    /// Adds a hero to the collection.
    /// </summary>
    /// <param name="item">The hero to add.</param>
    /// <param name="isSelected">Whether the hero should be selected after adding.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddHero(ImageViewModel item, bool isSelected = false)
    {
        InitializeHero(item);

        Heros.Add(item);

        item.MediaType = MediaType.Hero;

        await OnImageAdded(item);

        if (isSelected)
        {
            item.IsSelected = true;
            OnHeroSelectionChanged(item);
        }
    }

    /// <summary>
    /// Inserts a hero at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The hero to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertHero(int index, ImageViewModel item)
    {
        InitializeHero(item);

        Heros.Insert(index, item);

        item.MediaType = MediaType.Hero;

        await OnImageAdded(item);
    }

    /// <summary>
    /// Removes a hero from the collection.
    /// </summary>
    /// <param name="item">The hero to remove.</param>
    public void RemoveHero(ImageViewModel item)
    {
        DeInitializeHero(item);
        Heros.Remove(item);
    }

    /// <summary>
    /// Clears all heroes from the collection.
    /// </summary>
    public void ClearHeros()
    {
        foreach (ImageViewModel? item in Heros.ToList())
        {
            RemoveHero(item);
        }
    }

    /// <summary>
    /// Adds a slide to the collection.
    /// </summary>
    /// <param name="item">The slide to add.</param>
    /// <param name="isSelected">Whether the slide should be selected after adding.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddSlide(ImageViewModel item, bool isSelected = false)
    {
        InitializeSlide(item);

        Slides.Add(item);

        item.MediaType = MediaType.Slide;

        await OnImageAdded(item);

        if (isSelected)
        {
            item.IsSelected = true;
            OnSlideSelectionChanged(item);
        }
    }

    /// <summary>
    /// Inserts a slide at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The slide to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertSlide(int index, ImageViewModel item)
    {
        InitializeSlide(item);

        Slides.Insert(index, item);

        item.MediaType = MediaType.Slide;

        await OnImageAdded(item);
    }

    /// <summary>
    /// Removes a slide from the collection.
    /// </summary>
    /// <param name="item">The slide to remove.</param>
    public void RemoveSlide(ImageViewModel item)
    {
        DeInitializeSlide(item);
        Slides.Remove(item);
    }

    /// <summary>
    /// Clears all slides from the collection.
    /// </summary>
    public void ClearSlides()
    {
        foreach (ImageViewModel? item in Slides.ToList())
        {
            RemoveSlide(item);
        }
    }

    /// <summary>
    /// Adds a music item to the collection.
    /// </summary>
    /// <param name="item">The music item to add.</param>
    /// <param name="isSelected">Whether the music item should be selected after adding.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddMusic(MusicViewModel item, bool isSelected = false)
    {
        InitializeMusic(item);

        Music.Add(item);

        if (isSelected)
        {
            item.IsSelected = true;
            OnMusicSelectionChanged(item);
        }
    }

    /// <summary>
    /// Inserts a music item at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The music item to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertMusic(int index, MusicViewModel item)
    {
        InitializeMusic(item);

        Music.Insert(index, item);
    }

    /// <summary>
    /// Removes a music item from the collection.
    /// </summary>
    /// <param name="item">The music item to remove.</param>
    public void RemoveMusic(MusicViewModel item)
    {
        DeInitializeMusic(item);

        Music.Remove(item);
    }

    /// <summary>
    /// Clears all music items from the collection.
    /// </summary>
    public void ClearMusic()
    {
        foreach (MusicViewModel? item in Music.ToList())
        {
            RemoveMusic(item);
        }
    }

    /// <summary>
    /// Adds a video item to the collection.
    /// </summary>
    /// <param name="item">The video item to add.</param>
    /// <param name="isSelected">Whether the video item should be selected after adding.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddVideo(VideoViewModel item, bool isSelected = false)
    {
        InitializeVideo(item);

        Videos.Add(item);

        if (isSelected)
        {
            item.IsSelected = true;
            OnVideoSelectionChanged(item);
        }
    }

    /// <summary>
    /// Inserts a video item at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The video item to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InsertVideo(int index, VideoViewModel item)
    {
        InitializeVideo(item);

        Videos.Insert(index, item);
    }

    /// <summary>
    /// Removes a video item from the collection.
    /// </summary>
    /// <param name="item">The video item to remove.</param>
    public void RemoveVideo(VideoViewModel item)
    {
        DeInitializeVideo(item);

        Videos.Remove(item);
    }

    /// <summary>
    /// Clears all video items from the collection.
    /// </summary>
    public void ClearVideo()
    {
        foreach (VideoViewModel? item in Videos.ToList())
        {
            RemoveVideo(item);
        }
    }

    /// <summary>
    /// Requests removal of the media context.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the scraping service.
    /// </summary>
    protected IScrapingService ScrapingService { get; private set; }

    /// <summary>
    /// Gets the upscaler service.
    /// </summary>
    protected IUpscalerService UpscalerService { get; private set; }

    /// <summary>
    /// Gets the meida formatter service.
    /// </summary>
    protected IMediaFormatterService MediaFormatterService { get; private set; }

    /// <summary>
    /// Gets the file service.
    /// </summary>
    protected IFileService FileService { get; private set; }

    /// <summary>
    /// Gets the downloader service.
    /// </summary>
    protected IDownloader Downloader { get; private set; }

    /// <summary>
    /// Gets all media items in the context.
    /// </summary>
    public IEnumerable<MediaViewModel> AllMedia =>
    [
        .. Icons,
        .. Logos,
        .. Titles,
        .. Heros,
        .. Slides,
        .. Music,
        .. Videos
    ];

    /// <summary>
    /// Gets all non-selected media items.
    /// </summary>
    public IEnumerable<MediaViewModel> NonSelectedMedia => AllMedia.Where(m => !m.IsSelected);

    /// <summary>
    /// Gets all selected media items.
    /// </summary>
    public IEnumerable<MediaViewModel> SelectedMedia => AllMedia.Where(m => m.IsSelected);

    /// <summary>
    /// Gets all image media items.
    /// </summary>
    public IEnumerable<ImageViewModel> ImageMedia => AllMedia.OfType<ImageViewModel>();

    /// <summary>
    /// Gets all local image media items.
    /// </summary>
    public IEnumerable<ImageViewModel> LocalImageMedia => AllMedia.OfType<ImageViewModel>().Where(i => !string.IsNullOrWhiteSpace(i.LocalPath));

    /// <summary>
    /// Gets all non-selected image media items.
    /// </summary>
    public IEnumerable<ImageViewModel> NonSelectedImageMedia => NonSelectedMedia.OfType<ImageViewModel>();

    /// <summary>
    /// Gets all selected image media items.
    /// </summary>
    public IEnumerable<ImageViewModel> SelectedImageMedia => SelectedMedia.OfType<ImageViewModel>();
}
