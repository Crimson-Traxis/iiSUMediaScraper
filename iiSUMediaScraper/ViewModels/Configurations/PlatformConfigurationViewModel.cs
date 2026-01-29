using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for platform configuration settings.
/// </summary>
public partial class PlatformConfigurationViewModel : ObservablePlatformConfiguration
{
    /// <summary>
    /// Gets or sets the path to the platform icon.
    /// </summary>
    [ObservableProperty]
    private string iconPath;

    /// <summary>
    /// Raised when removal is requested for the configuration.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying platform configuration model.</param>
    public PlatformConfigurationViewModel(PlatformConfiguration baseModel) : base(baseModel)
    {
    }

    /// <summary>
    /// Requests removal of the platform configuration.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
