using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Contracts.Services;
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
    /// Gets or sets a value indicating whether the media is downloading.
    /// </summary>
    [ObservableProperty]
    private bool isDownloading;

    /// <summary>
    /// Gets or sets a value indicating whether the media is loading.
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// Gets or sets a value indicating progress.
    /// </summary>
    [ObservableProperty]
    private int progress;

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
    /// Gets or sets the crop settings for the media.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCrop))]
    private CropViewModel? crop;

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
    public MediaViewModel(Media baseModel, MediaType mediaType, IDownloader downloader, ConfigurationViewModel configuration) : base(baseModel)
    {
        MediaType = mediaType;

        Downloader = downloader;

        Configuration = configuration;

        if (!string.IsNullOrWhiteSpace(LocalPath))
        {
            TaskCompletionSource = new TaskCompletionSource();

            TaskCompletionSource.TrySetResult();
        }

        if (baseModel.Crop != null)
        {
            crop = new CropViewModel(baseModel.Crop);
        }
    }

    /// <summary>
    /// Called when the IsSelected property changes.
    /// </summary>
    /// <param name="value">The new selection state.</param>
    partial void OnIsSelectedChanged(bool value)
    {
        IsSelectedChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the Crop property changes.
    /// </summary>
    /// <param name="value">The new crop settings.</param>
    partial void OnCropChanged(CropViewModel? value)
    {
        BaseModel.Crop = value?.BaseModel;
    }

    /// <summary>
    /// Handles the progress updated event from the downloader.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The progress percentage.</param>
    private void Downloader_ProgressUpdated(object? sender, double e)
    {
        Progress = (int)e;
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
        OnPropertyChanged(nameof(LocalPath));
    }

    /// <summary>
    /// Downloads the media from its URL to a local file.
    /// If a download is already in progress, waits for it to complete.
    /// </summary>
    /// <returns>A task representing the download operation.</returns>
    public async Task Download()
    {
        if (TaskCompletionSource == null)
        {
            IsDownloading = true;

            TaskCompletionSource = new TaskCompletionSource();

            Downloader.ProgressUpdated += Downloader_ProgressUpdated;

            var tasks = new List<Task>();

            if (this is ImageViewModel imageViewModel)
            {
                tasks.Add(Downloader.DownloadImage(imageViewModel.BaseModel));
            }
            else if (this is VideoViewModel videoViewModel)
            {
                if(videoViewModel.Thumbnail != null)
                {
                    tasks.Add(videoViewModel.Thumbnail.Download());
                }

                tasks.Add(Downloader.DownloadVideo(videoViewModel.BaseModel));
            }
            else if (this is MusicViewModel musicViewModel)
            {
                if (musicViewModel.Thumbnail != null)
                {
                    tasks.Add(musicViewModel.Thumbnail.Download());
                }

                tasks.Add(Downloader.DownloadMusic(musicViewModel.BaseModel));
            }

            await Task.WhenAll(tasks);

            TaskCompletionSource.TrySetResult();

            OnPropertyChanged(nameof(LocalPath));

            Downloader.ProgressUpdated -= Downloader_ProgressUpdated;

            IsDownloading = false;
        }
        else
        {
            await TaskCompletionSource.Task;
        }
    }

    /// <summary>
    /// Waits for the media download to complete.
    /// </summary>
    /// <returns>A task representing the download operation.</returns>
    public Task WaitForDownload()
    {
        return Download();
    }

    /// <summary>
    /// Gets the task completion source for tracking download state.
    /// </summary>
    protected TaskCompletionSource TaskCompletionSource { get; private set; }

    /// <summary>
    /// Gets the downloader service for downloading media.
    /// </summary>
    protected IDownloader Downloader { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the media has crop settings.
    /// </summary>
    public bool HasCrop => Crop != null;
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
    protected MediaViewModel(T baseModel, MediaType mediaType, IDownloader downloader, ConfigurationViewModel configuration) : base(baseModel, mediaType, downloader, configuration)
    {
    }

    /// <summary>
    /// Gets the underlying media model.
    /// </summary>
    public override T BaseModel => (T)base.BaseModel;
}
