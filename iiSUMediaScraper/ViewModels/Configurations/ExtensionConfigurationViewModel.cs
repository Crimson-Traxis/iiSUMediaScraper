using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for file extension configuration settings.
/// </summary>
public partial class ExtensionConfigurationViewModel : ObservableExtensionConfiguration
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
    /// Gets or sets the comma-delimited string of file extensions.
    /// </summary>
    [ObservableProperty]
    private string? extensionDilimited;

    /// <summary>
    /// Gets or sets the collection of file extensions.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> extensions;

    /// <summary>
    /// Gets the collection of available platform configurations.
    /// </summary>
    public ObservableCollection<PlatformConfigurationViewModel> PlatformConfigurations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying extension configuration model.</param>
    /// <param name="configurationViewModel">The configuration view model.</param>
    public ExtensionConfigurationViewModel(ExtensionConfiguration baseModel, ConfigurationViewModel configurationViewModel) : base(baseModel)
    {
        PlatformConfigurations = configurationViewModel.PlatformConfigurations;

        selectedPlatform = PlatformConfigurations.FirstOrDefault(p => p.Code == baseModel.Platform);

        extensions = [];

        RegisterObservableCollection(
            nameof(Extensions),
            baseModel.Extension,
            extensions);

        extensionDilimited = string.Join(", ", extensions);
    }

    /// <summary>
    /// Called when the extension delimited string changes to parse and update the extensions collection.
    /// </summary>
    /// <param name="value">The new comma-delimited string of extensions.</param>
    partial void OnExtensionDilimitedChanged(string? value)
    {
        if (value != null)
        {
            Extensions.Clear();

            foreach (string extension in value.Split(",", StringSplitOptions.RemoveEmptyEntries))
            {
                Extensions.Add(extension.Trim());
            }
        }
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
    /// Creates a new empty extension string.
    /// </summary>
    /// <returns>An empty string representing a new extension.</returns>
    public string CreateExtension()
    {
        return string.Empty;
    }

    /// <summary>
    /// Creates a new file extension and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewExtension()
    {
        InsertExtension(0, CreateExtension());
    }

    /// <summary>
    /// Adds a file extension to the collection.
    /// </summary>
    /// <param name="item">The extension to add.</param>
    public void AddExtension(string item)
    {
        Extensions.Add(item);
    }

    /// <summary>
    /// Inserts a file extension at the specified index in the collection.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the extension.</param>
    /// <param name="item">The extension to insert.</param>
    public void InsertExtension(int index, string item)
    {
        Extensions.Insert(index, item);
    }

    /// <summary>
    /// Removes a file extension from the collection.
    /// </summary>
    /// <param name="item">The extension to remove.</param>
    public void RemoveExtension(string item)
    {
        Extensions.Remove(item);
    }

    /// <summary>
    /// Removes all file extensions from the collection.
    /// </summary>
    public void ClearExtensions()
    {
        foreach (string? item in Extensions.ToList())
        {
            RemoveExtension(item);
        }
    }

    /// <summary>
    /// Requests removal of the extension configuration.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
