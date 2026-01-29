using CommunityToolkit.Mvvm.ComponentModel;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using iiSUMediaScraper.ViewModels.Configurations;

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
}
