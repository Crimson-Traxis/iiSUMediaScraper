using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ObservableModels;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for music media items.
/// </summary>
public partial class MusicViewModel : MediaViewModel<Music>, IBaseObservableModel<Music>
{
    /// <summary>
    /// Gets or sets the thumbnail image for the music.
    /// </summary>
    [ObservableProperty]
    private ImageViewModel? thumbnail;

    /// <summary>
    /// Raised when playback is requested for the music.
    /// </summary>
    public event EventHandler PlayRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying music model.</param>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="configuration">The configuration view model.</param>
    public MusicViewModel(Music baseModel, MediaType mediaType, IDownloader downloader, ConfigurationViewModel configuration) : base(baseModel, mediaType, downloader, configuration)
    {
        if(BaseModel.Thumbnail != null)
        {
            thumbnail = new ImageViewModel(BaseModel.Thumbnail, MediaType.Thumbnail, downloader, configuration);
        }
    }

    /// <summary>
    /// Called when the Thumbnail property changes.
    /// </summary>
    /// <param name="value">The new thumbnail view model.</param>
    partial void OnThumbnailChanged(ImageViewModel value)
    {
        BaseModel.Thumbnail = value?.BaseModel;
    }

    /// <summary>
    /// Gets or sets the duration of the music.
    /// </summary>
    public TimeSpan Duration
    {
        get => BaseModel.Duration;
        set => SetProperty(BaseModel.Duration, value, BaseModel, (o, v) => o.Duration = v);
    }

    /// <summary>
    /// Gets or sets the likes of the music.
    /// </summary>
    public long LikeCount
    {
        get => BaseModel.LikeCount;
        set => SetProperty(BaseModel.LikeCount, value, BaseModel, (o, v) => o.LikeCount = v);
    }

    /// <summary>
    /// Gets or sets the Title of the music.
    /// </summary>
    public string? Title
    {
        get => BaseModel.Title;
        set => SetProperty(BaseModel.Title, value, BaseModel, (o, v) => o.Title = v);
    }

    /// <summary>
    /// Requests playback of the music and starts downloading if needed.
    /// </summary>
    [RelayCommand]
    public async Task RequestPlay()
    {
        var downloadTask = Download();

        PlayRequested?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Represents a generic view model for music items.
/// </summary>
/// <typeparam name="T">The type of music model.</typeparam>
public partial class MusicViewModel<T> : MusicViewModel
    where T : Music
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MusicViewModel{T}"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying media model.</param>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="configuration">The configuration view model.</param>
    protected MusicViewModel(T baseModel, MediaType mediaType, IDownloader downloader, ConfigurationViewModel configuration) : base(baseModel, mediaType, downloader, configuration)
    {
    }

    /// <summary>
    /// Gets the underlying media model.
    /// </summary>
    public override T BaseModel => (T)base.BaseModel;
}
