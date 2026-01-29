using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for game icon overlay configuration settings.
/// </summary>
public partial class GameIconOverlayConfigurationViewModel : ObservableGameIconOverlayConfiguration
{
    /// <summary>
    /// Raised when removal is requested for the configuration.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Gets or sets the selected platform configuration.
    /// </summary>
    [ObservableProperty]
    private PlatformConfigurationViewModel? selectedPlatform;

    /// <summary>
    /// Gets the collection of available platform configurations.
    /// </summary>
    public ObservableCollection<PlatformConfigurationViewModel> PlatformConfigurations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameIconOverlayConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying game icon overlay configuration model.</param>
    /// <param name="configurationViewModel">The configuration view model.</param>
    public GameIconOverlayConfigurationViewModel(GameIconOverlayConfiguration baseModel, ConfigurationViewModel configurationViewModel) : base(baseModel)
    {
        PlatformConfigurations = configurationViewModel.PlatformConfigurations;

        selectedPlatform = PlatformConfigurations.FirstOrDefault(p => p.Code == baseModel.Platform);
    }

    partial void OnSelectedPlatformChanged(PlatformConfigurationViewModel? value)
    {
        if (value != null)
        {
            Platform = value.Code;
        }
    }

    /// <summary>
    /// Requests removal of the game icon overlay configuration.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
