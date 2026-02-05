using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ObservableModels;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using System.ComponentModel;
using System.IO;

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
    /// <param name="configuration">The configuration view model.</param>
    public ImageViewModel(Image baseModel, MediaType mediaType, IDownloader downloader, ConfigurationViewModel configuration) : base(baseModel, mediaType, downloader, configuration)
    {
        aspectRatio = GetAspectRatio(mediaType, configuration);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying image model.</param>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="mediaFormatterService">The media formatter service.</param>
    /// <param name="upscalerService">The upscaler service.</param>
    /// <param name="configuration">The configuration view model.</param>
    public ImageViewModel(Image baseModel, MediaType mediaType, IMediaFormatterService mediaFormatterService, IUpscalerService upscalerService, IDownloader downloader, ConfigurationViewModel configuration) : this(baseModel, mediaType, downloader, configuration)
    {
        MediaFormatterService = mediaFormatterService;

        UpscalerService = upscalerService;
    }

    /// <summary>
    /// Gets the target width and height based on the media type and configuration.
    /// </summary>
    /// <returns>A tuple containing the target width and height.</returns>
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

    /// <summary>
    /// Gets the aspect ratio for the specified media type from configuration.
    /// </summary>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="configuration">The configuration view model.</param>
    /// <returns>The aspect ratio value, or null if not configured.</returns>
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

    /// <summary>
    /// Called when a property value changes.
    /// </summary>
    /// <param name="e">The property changed event args.</param>
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
        if (UpscalerService != null && MediaFormatterService != null)
        {
            IsUpscaling = true;

            (int, int) target = GetTargetWidthHeight();

            // Read bytes from LocalPath
            var imageBytes = !string.IsNullOrWhiteSpace(LocalPath) && File.Exists(LocalPath)
                ? await File.ReadAllBytesAsync(LocalPath)
                : [];

            if (imageBytes.Length == 0)
            {
                HasUpscaleError = true;
                IsUpscaling = false;
                return;
            }

            var response = await UpscalerService.UpscaleAsync(upscalerConfiguration.BaseModel, imageBytes, target.Item1, target.Item2);

            if (response.Success && response.ImageData != null && response.Width != null && response.Height != null && Crop != null)
            {
                // Write result to new temp file
                var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                await File.WriteAllBytesAsync(tempPath, response.ImageData);
                LocalPath = tempPath;

                var newCrop = await MediaFormatterService.CalculateNewCrop(Width, Height, Crop.BaseModel, (int)response.Width, (int)response.Height);

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
    }

    /// <summary>
    /// Gets the upscaler service.
    /// </summary>
    protected IUpscalerService? UpscalerService { get; private set; }

    /// <summary>
    /// Gets the meida formatter service.
    /// </summary>
    protected IMediaFormatterService? MediaFormatterService { get; private set; }
}
