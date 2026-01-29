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

    partial void OnSelectedPlatformChanged(PlatformConfigurationViewModel? value)
    {
        if (value != null)
        {
            Platform = value.Code;
        }
    }

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

    public void AddExtension(string item)
    {
        Extensions.Add(item);
    }

    public void InsertExtension(int index, string item)
    {
        Extensions.Insert(index, item);
    }

    public void RemoveExtension(string item)
    {
        Extensions.Remove(item);
    }

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
