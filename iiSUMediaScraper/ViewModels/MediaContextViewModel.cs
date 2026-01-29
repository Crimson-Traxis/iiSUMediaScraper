using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ObservableModels;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
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
    private ObservableCollection<MediaViewModel> heros;

    /// <summary>
    /// Gets or sets the collection of slide media items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MediaViewModel> slides;

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
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContextViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying media context model.</param>
    /// <param name="scrapingService">The scraping service.</param>
    /// <param name="imageFormatterService">The image formatter service.</param>
    /// <param name="upscalerService">The upscaler service.</param>
    /// <param name="fileService">The file service.</param>
    /// <param name="configuration">The configuration view model.</param>
    public MediaContextViewModel(MediaContext baseModel,
                                 IScrapingService scrapingService,
                                 IImageFormatterService imageFormatterService,
                                 IUpscalerService upscalerService,
                                 IFileService fileService,
                                 ConfigurationViewModel configuration) : base(baseModel)
    {
        ScrapingService = scrapingService;

        ImageFormatterService = imageFormatterService;

        UpscalerService = upscalerService;

        FileService = fileService;

        Configuration = configuration;

        icons = [];
        logos = [];
        titles = [];
        heros = [];
        slides = [];

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

        if ((Configuration.IsPrioritizeLogoAsTitle || Configuration.IsUseLogoAsTitle) && logos.Any())
        {
            logos.First().IsSelected = true;
        }
        else if (titles.Any() && !Configuration.IsUseLogoAsTitle)
        {
            titles.First().IsSelected = true;
        }

        if (icons.Any())
        {
            icons.First().IsSelected = true;
        }

        if (heros.Any())
        {
            heros.First().IsSelected = true;
        }

        foreach (MediaViewModel slide in slides)
        {
            slide.IsSelected = true;
        }
    }

    private void OnPreviewRequested(object? sender, ImageViewModel e)
    {
        PreviewRequested?.Invoke(this, e);
    }

    private void OnStopPreviewRequested(object? sender, ImageViewModel e)
    {
        StopPreviewRequested?.Invoke(this, e);
    }

    private void OnEditRequested(object? sender, ImageViewModel e)
    {
        EditRequested?.Invoke(this, e);
    }

    private void Icon_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is MediaViewModel mediaViewModel)
        {
            if (mediaViewModel.IsSelected)
            {
                foreach (MediaViewModel? icon in Icons.Except([mediaViewModel]))
                {
                    icon.IsSelected = false;
                }
            }
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Logo_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is MediaViewModel mediaViewModel)
        {
            if (mediaViewModel.IsSelected)
            {
                foreach (ImageViewModel title in Titles)
                {
                    title.IsSelected = false;
                }

                foreach (MediaViewModel? logo in Logos.Except([mediaViewModel]))
                {
                    logo.IsSelected = false;
                }
            }
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Title_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is MediaViewModel mediaViewModel)
        {
            if (mediaViewModel.IsSelected)
            {
                foreach (MediaViewModel? title in Titles.Except([mediaViewModel]))
                {
                    title.IsSelected = false;
                }

                foreach (ImageViewModel logo in Logos)
                {
                    logo.IsSelected = false;
                }
            }
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Hero_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is MediaViewModel mediaViewModel)
        {
            if (mediaViewModel.IsSelected)
            {

            }
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Slide_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is MediaViewModel mediaViewModel)
        {
            if (mediaViewModel.IsSelected)
            {

            }
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnIconRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel item)
        {
            RemoveIcon(item);
        }
    }

    private void OnLogoRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel item)
        {
            RemoveLogo(item);
        }
    }

    private void OnTitleRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is ImageViewModel item)
        {
            RemoveTitle(item);
        }
    }

    private void OnHeroRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is MediaViewModel item)
        {
            RemoveHero(item);
        }
    }

    private void OnSlideRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is MediaViewModel item)
        {
            RemoveSlide(item);
        }
    }

    private async Task ReadLocalData(MediaViewModel mediaViewModel)
    {
        if (!string.IsNullOrEmpty(mediaViewModel.Url))
        {
            mediaViewModel.Bytes = await FileService.ReadBytes(mediaViewModel.Url);
        }
    }

    private async Task OnImageAdded(ImageViewModel imageViewModel)
    {
        if (imageViewModel.Width == 0 || imageViewModel.Height == 0)
        {
            await ImageFormatterService.UpdateImageSize(imageViewModel.BaseModel);
        }

        if (imageViewModel.Crop == null)
        {
            Crop? crop = null;

            switch (imageViewModel.MediaType)
            {
                case MediaType.Icon:
                    crop = await ImageFormatterService.SmartCropIcon(imageViewModel.BaseModel);
                    break;
                case MediaType.Logo:
                    crop = await ImageFormatterService.SmartCropLogo(imageViewModel.BaseModel);
                    break;
                case MediaType.Title:
                    crop = await ImageFormatterService.SmartCropTitle(imageViewModel.BaseModel);
                    break;
                case MediaType.Hero:
                    crop = await ImageFormatterService.SmartCropHero(imageViewModel.BaseModel);
                    break;
                case MediaType.Slide:
                    crop = await ImageFormatterService.SmartCropSlide(imageViewModel.BaseModel);
                    break;
            }

            if (crop != null)
            {
                imageViewModel.Crop = new CropViewModel(crop);
            }
        }
    }

    protected void InitializeIcon(ImageViewModel item)
    {
        item.RemoveRequested += OnIconRemoveRequested;
        item.IsSelectedChanged += Icon_IsSelectedChanged;
        item.PreviewRequested += OnPreviewRequested;
        item.StopPreviewRequested += OnStopPreviewRequested;
        item.EditRequested += OnEditRequested;
    }

    protected void DeInitializeIcon(ImageViewModel item)
    {
        item.RemoveRequested -= OnIconRemoveRequested;
        item.IsSelectedChanged -= Icon_IsSelectedChanged;
        item.PreviewRequested -= OnPreviewRequested;
        item.StopPreviewRequested -= OnStopPreviewRequested;
        item.EditRequested -= OnEditRequested;
    }

    protected void InitializeLogo(ImageViewModel item)
    {
        item.RemoveRequested += OnLogoRemoveRequested;
        item.IsSelectedChanged += Logo_IsSelectedChanged;
        item.PreviewRequested += OnPreviewRequested;
        item.StopPreviewRequested += OnStopPreviewRequested;
        item.EditRequested += OnEditRequested;
    }

    protected void DeInitializeLogo(ImageViewModel item)
    {
        item.RemoveRequested -= OnLogoRemoveRequested;
        item.IsSelectedChanged -= Logo_IsSelectedChanged;
        item.PreviewRequested -= OnPreviewRequested;
        item.StopPreviewRequested -= OnStopPreviewRequested;
        item.EditRequested -= OnEditRequested;
    }

    protected void InitializeTitle(ImageViewModel item)
    {
        item.RemoveRequested += OnTitleRemoveRequested;
        item.IsSelectedChanged += Title_IsSelectedChanged;
        item.PreviewRequested += OnPreviewRequested;
        item.StopPreviewRequested += OnStopPreviewRequested;
        item.EditRequested += OnEditRequested;
    }

    protected void DeInitializeTitle(ImageViewModel item)
    {
        item.RemoveRequested -= OnTitleRemoveRequested;
        item.IsSelectedChanged -= Title_IsSelectedChanged;
        item.PreviewRequested -= OnPreviewRequested;
        item.StopPreviewRequested -= OnStopPreviewRequested;
        item.EditRequested -= OnEditRequested;
    }

    protected void InitializeHero(MediaViewModel item)
    {
        item.RemoveRequested += OnHeroRemoveRequested;
        item.IsSelectedChanged += Hero_IsSelectedChanged;

        if (item is ImageViewModel imageViewModel)
        {
            imageViewModel.PreviewRequested += OnPreviewRequested;
            imageViewModel.StopPreviewRequested += OnStopPreviewRequested;
            imageViewModel.EditRequested += OnEditRequested;
        }
    }

    protected void DeInitializeHero(MediaViewModel item)
    {
        item.RemoveRequested -= OnHeroRemoveRequested;
        item.IsSelectedChanged -= Hero_IsSelectedChanged;

        if (item is ImageViewModel imageViewModel)
        {
            imageViewModel.PreviewRequested -= OnPreviewRequested;
            imageViewModel.StopPreviewRequested -= OnStopPreviewRequested;
            imageViewModel.EditRequested -= OnEditRequested;
        }
    }

    protected void InitializeSlide(MediaViewModel item)
    {
        item.RemoveRequested += OnSlideRemoveRequested;
        item.IsSelectedChanged += Slide_IsSelectedChanged;

        if (item is ImageViewModel imageViewModel)
        {
            imageViewModel.PreviewRequested += OnPreviewRequested;
            imageViewModel.StopPreviewRequested += OnStopPreviewRequested;
            imageViewModel.EditRequested += OnEditRequested;
        }
    }

    protected void DeInitializeSlide(MediaViewModel item)
    {
        item.RemoveRequested -= OnSlideRemoveRequested;
        item.IsSelectedChanged -= Slide_IsSelectedChanged;

        if (item is ImageViewModel imageViewModel)
        {
            imageViewModel.PreviewRequested -= OnPreviewRequested;
            imageViewModel.StopPreviewRequested -= OnStopPreviewRequested;
            imageViewModel.EditRequested -= OnEditRequested;
        }
    }

    public ImageViewModel CreateIcon(Image baseModel)
    {
        return new ImageViewModel(baseModel, MediaType.Icon, ImageFormatterService, UpscalerService, Configuration);
    }

    public ImageViewModel CreateLogo(Image baseModel)
    {
        return new ImageViewModel(baseModel, MediaType.Logo, ImageFormatterService, UpscalerService, Configuration);
    }

    public ImageViewModel CreateTitle(Image baseModel)
    {
        return new ImageViewModel(baseModel, MediaType.Title, ImageFormatterService, UpscalerService, Configuration);
    }

    public MediaViewModel CreateHero(Media baseModel)
    {
        if (baseModel is Image image)
        {
            return new ImageViewModel(image, MediaType.Hero, ImageFormatterService, UpscalerService, Configuration);
        }
        else if (baseModel is Video video)
        {
            return new VideoViewModel(video, MediaType.Hero, Configuration);
        }

        return new MediaViewModel(baseModel, MediaType.Hero, Configuration);
    }

    public MediaViewModel CreateSlide(Media baseModel)
    {
        if (baseModel is Image image)
        {
            return new ImageViewModel(image, MediaType.Slide, ImageFormatterService, UpscalerService, Configuration);
        }
        else if (baseModel is Video video)
        {
            return new VideoViewModel(video, MediaType.Slide, Configuration);
        }

        return new MediaViewModel(baseModel, MediaType.Slide, Configuration);
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
        await InsertHero(0, CreateHero(new Media()));
    }

    /// <summary>
    /// Creates a new slide and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public async Task CreateNewSlide()
    {
        await InsertSlide(0, CreateSlide(new Media()));
    }

    public async Task AddIcon(ImageViewModel item)
    {
        InitializeIcon(item);

        Icons.Add(item);

        item.MediaType = MediaType.Icon;

        await OnImageAdded(item);
    }

    public async Task InsertIcon(int index, ImageViewModel item)
    {
        InitializeIcon(item);

        Icons.Insert(index, item);

        item.MediaType = MediaType.Icon;

        await OnImageAdded(item);
    }

    public void RemoveIcon(ImageViewModel item)
    {
        DeInitializeIcon(item);
        Icons.Remove(item);
    }

    public void ClearIcons()
    {
        foreach (ImageViewModel? item in Icons.ToList())
        {
            RemoveIcon(item);
        }
    }

    public async Task AddLogo(ImageViewModel item)
    {
        InitializeLogo(item);

        Logos.Add(item);

        item.MediaType = MediaType.Logo;

        await OnImageAdded(item);
    }

    public async Task InsertLogo(int index, ImageViewModel item)
    {
        InitializeLogo(item);

        Logos.Insert(index, item);

        item.MediaType = MediaType.Logo;

        await OnImageAdded(item);
    }

    public void RemoveLogo(ImageViewModel item)
    {
        DeInitializeLogo(item);
        Logos.Remove(item);
    }

    public void ClearLogos()
    {
        foreach (ImageViewModel? item in Logos.ToList())
        {
            RemoveLogo(item);
        }
    }

    public async Task AddTitle(ImageViewModel item)
    {
        InitializeTitle(item);

        Titles.Add(item);

        item.MediaType = MediaType.Title;

        await OnImageAdded(item);
    }

    public async Task InsertTitle(int index, ImageViewModel item)
    {
        InitializeTitle(item);

        Titles.Insert(index, item);

        item.MediaType = MediaType.Title;

        await OnImageAdded(item);
    }

    public void RemoveTitle(ImageViewModel item)
    {
        DeInitializeTitle(item);
        Titles.Remove(item);
    }

    public void ClearTitles()
    {
        foreach (ImageViewModel? item in Titles.ToList())
        {
            RemoveTitle(item);
        }
    }

    public async Task AddHero(MediaViewModel item)
    {
        InitializeHero(item);

        Heros.Add(item);

        item.MediaType = MediaType.Hero;

        if (item is ImageViewModel imageViewModel)
        {
            await OnImageAdded(imageViewModel);
        }
    }

    public async Task InsertHero(int index, MediaViewModel item)
    {
        InitializeHero(item);

        Heros.Insert(index, item);

        item.MediaType = MediaType.Hero;

        if (item is ImageViewModel imageViewModel)
        {
            await OnImageAdded(imageViewModel);
        }
    }

    public void RemoveHero(MediaViewModel item)
    {
        DeInitializeHero(item);
        Heros.Remove(item);
    }

    public void ClearHeros()
    {
        foreach (MediaViewModel? item in Heros.ToList())
        {
            RemoveHero(item);
        }
    }

    public async Task AddSlide(MediaViewModel item)
    {
        InitializeSlide(item);

        Slides.Add(item);

        item.MediaType = MediaType.Slide;

        if (item is ImageViewModel imageViewModel)
        {
            await OnImageAdded(imageViewModel);
        }
    }

    public async Task InsertSlide(int index, MediaViewModel item)
    {
        InitializeSlide(item);

        Slides.Insert(index, item);

        item.MediaType = MediaType.Slide;

        if (item is ImageViewModel imageViewModel)
        {
            await OnImageAdded(imageViewModel);
        }
    }

    public void RemoveSlide(MediaViewModel item)
    {
        DeInitializeSlide(item);
        Slides.Remove(item);
    }

    public void ClearSlides()
    {
        foreach (MediaViewModel? item in Slides.ToList())
        {
            RemoveSlide(item);
        }
    }

    /// <summary>
    /// Downloads image data for all media items.
    /// </summary>
    public async Task DownloadImageData()
    {
        foreach (MediaViewModel media in AllMedia)
        {
            media.IsLoading = true;
        }

        List<Task> tasks = new List<Task>
        {
            ScrapingService.DownloadMissingMedia(BaseModel)
        };

        foreach (ImageViewModel media in LocalImageMedia)
        {
            tasks.Add(ReadLocalData(media));
        }

        await Task.WhenAll(tasks);

        foreach (MediaViewModel media in AllMedia)
        {
            media.NotifyDataChanged();

            media.IsLoading = false;
        }
    }

    /// <summary>
    /// Clears image data for all non-selected media items.
    /// </summary>
    public void ClearImageData()
    {
        foreach (ImageViewModel media in NonSelectedImageMedia)
        {
            media.Bytes = [];
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
    /// Gets the image formatter service.
    /// </summary>
    protected IImageFormatterService ImageFormatterService { get; private set; }

    /// <summary>
    /// Gets the file service.
    /// </summary>
    protected IFileService FileService { get; private set; }

    /// <summary>
    /// Gets all media items in the context.
    /// </summary>
    public IEnumerable<MediaViewModel> AllMedia =>
    [
        .. Icons,
        .. Logos,
        .. Titles,
        .. Heros,
        .. Slides
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
    public IEnumerable<ImageViewModel> LocalImageMedia => AllMedia.OfType<ImageViewModel>().Where(i => i.Source == SourceFlag.Local);

    /// <summary>
    /// Gets all non-selected image media items.
    /// </summary>
    public IEnumerable<ImageViewModel> NonSelectedImageMedia => NonSelectedMedia.OfType<ImageViewModel>();

    /// <summary>
    /// Gets all selected image media items.
    /// </summary>
    public IEnumerable<ImageViewModel> SelectedImageMedia => SelectedMedia.OfType<ImageViewModel>();
}
