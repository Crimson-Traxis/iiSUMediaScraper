using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ObservableModels;
using iiSUMediaScraper.ViewModels.Configurations;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the base view model for media items.
/// </summary>
public partial class MediaViewModel : ObservableMedia
{
    /// <summary>
    /// Gets or sets a value indicating whether the media is selected.
    /// </summary>
    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// Gets or sets a value indicating whether the media is loading.
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// Gets or sets the media type.
    /// </summary>
    [ObservableProperty]
    private MediaType mediaType;

    /// <summary>
    /// Gets or sets the configuration view model.
    /// </summary>
    [ObservableProperty]
    private ConfigurationViewModel configuration;

    /// <summary>
    /// Raised when the selection state changes.
    /// </summary>
    public event EventHandler IsSelectedChanged;

    /// <summary>
    /// Raised when removal is requested for the media.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying media model.</param>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="configuration">The configuration view model.</param>
    public MediaViewModel(Media baseModel, MediaType mediaType, ConfigurationViewModel configuration) : base(baseModel)
    {
        MediaType = mediaType;

        Configuration = configuration;
    }

    partial void OnIsSelectedChanged(bool value)
    {
        IsSelectedChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Toggles the selection state of the media.
    /// </summary>
    [RelayCommand]
    public async Task ToggleSelect()
    {
        if (!IsLoading)
        {
            IsSelected = !IsSelected;
        }
    }

    /// <summary>
    /// Requests removal of the media.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Notifies that the media data has changed.
    /// </summary>
    public void NotifyDataChanged()
    {
        OnPropertyChanged(nameof(Bytes));
    }
}

/// <summary>
/// Represents a generic view model for media items.
/// </summary>
/// <typeparam name="T">The type of media model.</typeparam>
public partial class MediaViewModel<T> : MediaViewModel
    where T : Media
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaViewModel{T}"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying media model.</param>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="configuration">The configuration view model.</param>
    protected MediaViewModel(T baseModel, MediaType mediaType, ConfigurationViewModel configuration) : base(baseModel, mediaType, configuration)
    {
    }

    /// <summary>
    /// Gets the underlying media model.
    /// </summary>
    public override T BaseModel => (T)base.BaseModel;
}

/// <summary>
/// Specifies the type of media.
/// </summary>
public enum MediaType
{
    /// <summary>
    /// Icon media type.
    /// </summary>
    Icon,
    /// <summary>
    /// Logo media type.
    /// </summary>
    Logo,
    /// <summary>
    /// Title media type.
    /// </summary>
    Title,
    /// <summary>
    /// Hero media type.
    /// </summary>
    Hero,
    /// <summary>
    /// Slide media type.
    /// </summary>
    Slide
}
