using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for platform translation configuration settings.
/// </summary>
public partial class PlatformTranslationConfigurationViewModel : ObservablePlatformTranslationConfiguration
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
    /// Initializes a new instance of the <see cref="PlatformTranslationConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying platform translation configuration model.</param>
    /// <param name="configurationViewModel">The configuration view model.</param>
    public PlatformTranslationConfigurationViewModel(PlatformTranslationConfiguration baseModel, ConfigurationViewModel configurationViewModel) : base(baseModel)
    {
        PlatformConfigurations = configurationViewModel.PlatformConfigurations;

        selectedPlatform = PlatformConfigurations.FirstOrDefault(p => p.Code == baseModel.Platform);
    }

    /// <summary>
    /// Called when the selected platform changes to update the platform code.
    /// </summary>
    /// <param name="value">The new selected platform configuration.</param>
    partial void OnSelectedPlatformChanged(PlatformConfigurationViewModel? value)
    {
        if (value != null)
        {
            Platform = value.Code;
        }
    }

    /// <summary>
    /// Requests removal of the platform translation configuration.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
