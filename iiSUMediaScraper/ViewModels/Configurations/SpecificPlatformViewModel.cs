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
