using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.ViewModels.Configurations;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for configuring image upscaling operations.
/// </summary>
public partial class ImageUpscaleConfigurationViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the image view model to be upscaled.
    /// </summary>
    [ObservableProperty]
    private ImageViewModel imageViewModel;

    /// <summary>
    /// Gets or sets the upscaler configuration to use for the upscaling operation.
    /// </summary>
    [ObservableProperty]
    private UpscalerConfigurationViewModel upscalerConfigurationViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageUpscaleConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="imageViewModel">The image view model to be upscaled.</param>
    /// <param name="upscalerConfigurationViewModel">The upscaler configuration to use.</param>
    public ImageUpscaleConfigurationViewModel(ImageViewModel imageViewModel, UpscalerConfigurationViewModel upscalerConfigurationViewModel)
    {
        ImageViewModel = imageViewModel;

        UpscalerConfigurationViewModel = upscalerConfigurationViewModel;
    }

    /// <summary>
    /// Upscales the image using the configured upscaler settings.
    /// </summary>
    [RelayCommand]
    public async Task Upscale()
    {
        await ImageViewModel.Upscale(UpscalerConfigurationViewModel);
    }
}
