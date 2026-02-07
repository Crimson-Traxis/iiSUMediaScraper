using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for a path history entry with remove support.
/// </summary>
public partial class PathHistoryViewModel : ObservableObject
{
    /// <summary>
    /// Raised when removal is requested for the path history entry.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Gets or sets the path value.
    /// </summary>
    [ObservableProperty]
    private string path;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathHistoryViewModel"/> class.
    /// </summary>
    /// <param name="path">The path value.</param>
    public PathHistoryViewModel(string path)
    {
        this.path = path;
    }

    /// <summary>
    /// Returns the path as the string representation.
    /// </summary>
    public override string ToString() => Path;

    /// <summary>
    /// Requests removal of the path history entry.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
