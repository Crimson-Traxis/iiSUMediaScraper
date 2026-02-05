using CommunityToolkit.Mvvm.ComponentModel;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using iiSUMediaScraper.Services;
using iiSUMediaScraper.ViewModels.Configurations;
using System.Text.RegularExpressions;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for a specific game selection.
/// </summary>
public partial class SpecificGameViewModel : ObservableSpecificGame
{
    /// <summary>
    /// Raised when removal is requested for the game.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Raised when the selection state changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Gets or sets the file path of the game.
    /// </summary>
    [ObservableProperty]
    private string path;

    /// <summary>
    /// Gets or sets a value indicating whether the game is selected.
    /// </summary>
    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// Gets or sets the platform configuration for the game.
    /// </summary>
    [ObservableProperty]
    private PlatformConfigurationViewModel? platform;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificGameViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying specific game model.</param>
    public SpecificGameViewModel(SpecificGame baseModel) : base(baseModel)
    {

    }

    /// <summary>
    /// Removes parenthetical region/version info from a game name.
    /// Example: "Super Mario Bros (USA)" becomes "Super Mario Bros"
    /// </summary>
    /// <param name="name">The game name to clean.</param>
    /// <returns>Cleaned game name.</returns>
    protected string CleanName(string name)
    {
        // Remove only trailing parenthetical groups (region info, version, etc.)
        string pattern = @"(\s*\([^)]*\))+$";
        name = Regex.Replace(name, pattern, string.Empty).Trim();

        return name;
    }

    /// <summary>
    /// Gets the formatted name of the game without file extension.
    /// </summary>
    public string FormattedName => CleanName(Name);
}
