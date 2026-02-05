using CommunityToolkit.Mvvm.ComponentModel;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for a specific platform selection.
/// </summary>
public partial class SpecificPlatformViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the platform is selected.
    /// </summary>
    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// Gets or sets the path to the platform icon.
    /// </summary>
    [ObservableProperty]
    private string iconPath;

    /// <summary>
    /// Gets or sets the platform configuration.
    /// </summary>
    [ObservableProperty]
    private PlatformConfigurationViewModel platformConfiguration;

    /// <summary>
    /// Raised when the selection state changes.
    /// </summary>
    public event EventHandler? SelectedChanged;

    /// <summary>
    /// Called when the platform configuration changes to manage property change subscriptions.
    /// </summary>
    /// <param name="oldValue">The previous platform configuration.</param>
    /// <param name="newValue">The new platform configuration.</param>
    partial void OnPlatformConfigurationChanged(PlatformConfigurationViewModel? oldValue, PlatformConfigurationViewModel newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= Platform_PropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += Platform_PropertyChanged;
        }
    }

    /// <summary>
    /// Handles property changes from the platform configuration to update the Code property.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments containing the property name.</param>
    private void Platform_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Code));
    }

    /// <summary>
    /// Gets the platform code derived from the platform name.
    /// </summary>
    public string Code
    {
        get
        {
            string name = PlatformConfiguration.Name;

            if (string.IsNullOrWhiteSpace(name))
                return "";

            string[] words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return words.Length >= 2
                ? $"{words[0][0]}{words[1][0]}".ToUpperInvariant()
                : name.Length >= 2
                    ? name[..2].ToUpperInvariant()
                    : name.ToUpperInvariant();
        }
    }
}
