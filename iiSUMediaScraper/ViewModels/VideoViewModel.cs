using CommunityToolkit.Mvvm.ComponentModel;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ObservableModels;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for video media items.
/// </summary>
public partial class VideoViewModel : MusicViewModel<Video>, IBaseObservableModel<Video>
{
    /// <summary>
    /// Gets or sets the collection of available media types for the video.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MediaType> mediaTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying video model.</param>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="configuration">The configuration view model.</param>
    public VideoViewModel(Video baseModel, MediaType mediaType, IDownloader downloader, ConfigurationViewModel configuration) : base(baseModel, mediaType, downloader, configuration)
    {
        mediaTypes = [MediaType.Slide, MediaType.Hero];
    }

    /// <summary>
    /// Gets or sets the media type the video will be applied to
    /// </summary>
    public MediaType ApplyMediaType
    {
        get => BaseModel.ApplyMediaType;
        set => SetProperty(BaseModel.ApplyMediaType, value, BaseModel, (o, v) => o.ApplyMediaType = v);
    }
}
