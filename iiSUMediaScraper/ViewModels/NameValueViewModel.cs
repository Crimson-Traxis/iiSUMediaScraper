using CommunityToolkit.Mvvm.ComponentModel;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents a view model for a name-value pair.
/// </summary>
public partial class NameValueViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [ObservableProperty]
    private string? name;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [ObservableProperty]
    private string? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="NameValueViewModel"/> class.
    /// </summary>
    public NameValueViewModel()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NameValueViewModel"/> class with the specified name and value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public NameValueViewModel(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
