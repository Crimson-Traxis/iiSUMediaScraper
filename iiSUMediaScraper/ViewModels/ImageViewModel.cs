using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ObservableModels;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using System.ComponentModel;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for image media items.
/// </summary>
public partial class ImageViewModel : MediaViewModel<Image>, IBaseObservableModel<Image>
{
    /// <summary>
    /// Gets or sets the aspect ratio for the image.
    /// </summary>
    [ObservableProperty]
    private double? aspectRatio;

    /// <summary>
    /// Gets or sets a value indicating whether the mouse is hovering over the image.
    /// </summary>
    [ObservableProperty]
    private bool isHovering;

    /// <summary>
    /// Gets or sets a value indicating whether an upscale error occurred.
    /// </summary>
    [ObservableProperty]
    private bool hasUpscaleError;

    /// <summary>
    /// Gets or sets a value indicating whether the image is currently being upscaled.
    /// </summary>
    [ObservableProperty]
    private bool isUpscaling;

    /// <summary>
    /// Gets or sets a value indicating whether the image is currently being reconstructed.
    /// </summary>
    [ObservableProperty]
    private bool isReconstructing;

    /// <summary>
    /// Gets or sets the crop settings for the image.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCrop))]
    private CropViewModel? crop;

    /// <summary>
    /// Raised when a preview is requested for the image.
    /// </summary>
    public event EventHandler<ImageViewModel>? PreviewRequested;

    /// <summary>
    /// Raised when stopping a preview is requested for the image.
    /// </summary>
    public event EventHandler<ImageViewModel>? StopPreviewRequested;

    /// <summary>
    /// Raised when editing is requested for the image.
    /// </summary>
    public event EventHandler<ImageViewModel>? EditRequested;

    /// <summary>
    /// Raised when image upscaling is completed.
    /// </summary>
    public event EventHandler<ImageViewModel>? UpscaleCompleted;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying image model.</param>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="imageFormatterService">The image formatter service.</param>
    /// <param name="upscalerService">The upscaler service.</param>
    /// <param name="configuration">The configuration view model.</param>
    public ImageViewModel(Image baseModel, MediaType mediaType, IImageFormatterService imageFormatterService, IUpscalerService upscalerService, ConfigurationViewModel configuration) : base(baseModel, mediaType, configuration)
    {
        ImageFormatterService = imageFormatterService;

        UpscalerService = upscalerService;

        aspectRatio = GetAspectRatio(mediaType, configuration);

        if (BaseModel.Crop != null)
        {
            crop = new CropViewModel(BaseModel.Crop);
        }
    }

    partial void OnCropChanged(CropViewModel? value)
    {
        BaseModel.Crop = value?.BaseModel;
    }

    private (int, int) GetTargetWidthHeight()
    {
        int targetWidth = Width;

        int targetHeight = Height;

        switch (MediaType)
        {
            case MediaType.Icon:
                if (!double.IsNaN(Configuration.IconWidth))
                {
                    targetWidth = (int)Configuration.IconWidth;
                }

                if (!double.IsNaN(Configuration.IconHeight))
                {
                    targetHeight = (int)Configuration.IconHeight;
                }
                break;
            case MediaType.Logo:
                if (!double.IsNaN(Configuration.LogoWidth))
                {
                    targetWidth = (int)Configuration.LogoWidth;
                }

                if (!double.IsNaN(Configuration.LogoHeight))
                {
                    targetHeight = (int)Configuration.LogoHeight;
                }
                break;
            case MediaType.Title:
                if (!double.IsNaN(Configuration.TitleWidth))
                {
                    targetWidth = (int)Configuration.TitleWidth;
                }

                if (!double.IsNaN(Configuration.TitleHeight))
                {
                    targetHeight = (int)Configuration.TitleHeight;
                }
                break;
            case MediaType.Hero:
                if (!double.IsNaN(Configuration.HeroWidth))
                {
                    targetWidth = (int)Configuration.HeroWidth;
                }

                if (!double.IsNaN(Configuration.HeroHeight))
                {
                    targetHeight = (int)Configuration.HeroHeight;
                }
                break;
            case MediaType.Slide:
                if (!double.IsNaN(Configuration.SlideWidth))
                {
                    targetWidth = (int)Configuration.SlideWidth;
                }

                if (!double.IsNaN(Configuration.SlideHeight))
                {
                    targetHeight = (int)Configuration.SlideHeight;
                }
                break;
        }

        return (targetWidth, targetHeight);
    }

    private double? GetAspectRatio(MediaType mediaType, ConfigurationViewModel configuration)
    {
        var (width, height) = mediaType switch
        {
            MediaType.Icon => (configuration.IconWidth, configuration.IconHeight),
            MediaType.Logo => (configuration.LogoWidth, configuration.LogoHeight),
            MediaType.Title => (configuration.TitleWidth, configuration.TitleHeight),
            MediaType.Hero => (configuration.HeroWidth, configuration.HeroHeight),
            MediaType.Slide => (configuration.SlideWidth, configuration.SlideHeight),
            _ => (0.0, 0.0)
        };

        return (width != 0 && height != 0 && !double.IsNaN(width) && !double.IsNaN(height)
            ? new AspectRatio((int)width, (int)height).Value
            : null);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsLoading):
                if (!IsLoading && IsHovering)
                {
                    _ = RequestPreview();
                }
                break;
        }

        base.OnPropertyChanged(e);
    }

    /// <summary>
    /// Gets or sets the width of the image.
    /// </summary>
    public int Width
    {
        get => BaseModel.Width;
        set => SetProperty(BaseModel.Width, value, BaseModel, (o, v) => o.Width = v);
    }

    /// <summary>
    /// Gets or sets the height of the image.
    /// </summary>
    public int Height
    {
        get => BaseModel.Height;
        set => SetProperty(BaseModel.Height, value, BaseModel, (o, v) => o.Height = v);
    }

    /// <summary>
    /// Requests editing mode for the image.
    /// </summary>
    [RelayCommand]
    public async Task RequestEdit()
    {
        if (IsLoading)
        {
            return;
        }

        EditRequested?.Invoke(this, this);
    }


    /// <summary>
    /// Requests a preview of the image.
    /// </summary>
    [RelayCommand]
    public async Task RequestPreview()
    {
        if (IsLoading)
        {
            return;
        }

        PreviewRequested?.Invoke(this, this);
    }

    /// <summary>
    /// Requests to stop previewing the image.
    /// </summary>
    [RelayCommand]
    public async Task RequestStopPreview()
    {
        if (IsLoading)
        {
            return;
        }

        StopPreviewRequested?.Invoke(this, this);
    }

    /// <summary>
    /// Upscales the image using the specified upscaler configuration.
    /// </summary>
    /// <param name="upscalerConfiguration">The upscaler configuration to use.</param>
    [RelayCommand]
    public async Task Upscale(UpscalerConfigurationViewModel upscalerConfiguration)
    {
        IsUpscaling = true;

        (int, int) target = GetTargetWidthHeight();

        var response = await UpscalerService.UpscaleAsync(upscalerConfiguration.BaseModel, Bytes, target.Item1, target.Item2);

        if (response.Success && response.ImageData != null && response.Width != null && response.Height != null && Crop != null)
        {
            Bytes = response.ImageData;

            var newCrop = await ImageFormatterService.CalculateNewCrop(Width, Height, Crop.BaseModel, (int)response.Width, (int)response.Height);

            if (newCrop != null)
            {
                Crop = new CropViewModel(newCrop);
            }

            Width = (int)response.Width;

            Height = (int)response.Height;

            UpscaleCompleted?.Invoke(this, this);
        }
        else
        {
            HasUpscaleError = true;
        }

        IsUpscaling = false;
    }

    /// <summary>
    /// Gets the upscaler service.
    /// </summary>
    protected IUpscalerService UpscalerService { get; private set; }

    /// <summary>
    /// Gets the image formatter service.
    /// </summary>
    protected IImageFormatterService ImageFormatterService { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the image has crop settings.
    /// </summary>
    public bool HasCrop => Crop != null;
}
