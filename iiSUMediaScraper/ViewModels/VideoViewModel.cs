using iiSUMediaScraper.Models;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for video media items.
/// </summary>
public partial class VideoViewModel : MediaViewModel<Video>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VideoViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying video model.</param>
    /// <param name="mediaType">The type of media.</param>
    /// <param name="configuration">The configuration view model.</param>
    public VideoViewModel(Video baseModel, MediaType mediaType, ConfigurationViewModel configuration) : base(baseModel, mediaType, configuration)
    {
    }

    /// <summary>
    /// Gets or sets the duration of the video.
    /// </summary>
    public TimeSpan Duration
    {
        get => BaseModel.Duration;
        set => SetProperty(BaseModel.Duration, value, BaseModel, (o, v) => o.Duration = v);
    }
}
