using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for folder name to platform mapping configuration.
/// </summary>
public partial class FolderNameConfigurationViewModel : ObservableFolderNameConfiguration
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
    /// Gets or sets the selected IsAssetFolder value as a string ("True" or "False").
    /// </summary>
    [ObservableProperty]
    private string selectedIsAssetFolder = "False";

    /// <summary>
    /// Gets the collection of available platform configurations.
    /// </summary>
    public ObservableCollection<PlatformConfigurationViewModel> PlatformConfigurations { get; }

    /// <summary>
    /// Gets the collection of available IsAssetFolder options.
    /// </summary>
    public IReadOnlyList<string> IsAssetFolderOptions { get; } = ["False", "True"];

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderNameConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying folder name configuration model.</param>
    /// <param name="configurationViewModel">The configuration view model.</param>
    public FolderNameConfigurationViewModel(FolderNameConfiguration baseModel, ConfigurationViewModel configurationViewModel) : base(baseModel)
    {
        PlatformConfigurations = configurationViewModel.PlatformConfigurations;

        selectedPlatform = PlatformConfigurations.FirstOrDefault(p => p.Code == baseModel.Platform);
        selectedIsAssetFolder = baseModel.IsAssetFolder ? "True" : "False";
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
    /// Called when the selected IsAssetFolder string changes to update the bool property.
    /// </summary>
    /// <param name="value">The new value ("True" or "False").</param>
    partial void OnSelectedIsAssetFolderChanged(string value)
    {
        IsAssetFolder = value == "True";
    }

    /// <summary>
    /// Requests removal of the folder name configuration.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
