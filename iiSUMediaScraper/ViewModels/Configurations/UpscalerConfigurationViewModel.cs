using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for upscaler configuration settings.
/// </summary>
public partial class UpscalerConfigurationViewModel : ObservableUpscalerConfiguration
{
    /// <summary>
    /// Gets or sets the selected color correction method.
    /// </summary>
    [ObservableProperty]
    private NameValueViewModel? selectedColorCorrection;

    /// <summary>
    /// Gets or sets the collection of available color correction methods.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<NameValueViewModel> colorCorrections;

    /// <summary>
    /// Raised when removal is requested for the configuration.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpscalerConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying upscaler configuration model.</param>
    public UpscalerConfigurationViewModel(UpscalerConfiguration baseModel) : base(baseModel)
    {
        colorCorrections =
        [
            new NameValueViewModel("Lab", "lab"),
            new NameValueViewModel("Wavelet", "wavelet"),
            new NameValueViewModel("Wavelet Adaptive", "wavelet_adaptive"),
            new NameValueViewModel("Hsv", "hsv"),
            new NameValueViewModel("Adain", "adain"),
            new NameValueViewModel("None", "none"),
        ];

        selectedColorCorrection = colorCorrections.FirstOrDefault(c => c.Value == BaseModel.ColorCorrection);
    }

    partial void OnSelectedColorCorrectionChanged(NameValueViewModel value)
    {
        BaseModel.ColorCorrection = value?.Value;
    }

    /// <summary>
    /// Requests removal of the upscaler configuration.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
