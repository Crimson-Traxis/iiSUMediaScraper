namespace iiSUMediaScraper.Models;

/// <summary>
/// Contains collections of different media types associated with a game.
/// </summary>
public class MediaContext
{
    /// <summary>
    /// Gets or sets the collection of icon images.
    /// </summary>
    public List<Image> Icons { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of logo images.
    /// </summary>
    public List<Image> Logos { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of title images.
    /// </summary>
    public List<Image> Titles { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of hero images.
    /// </summary>
    public List<Image> Heros { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of slide images.
    /// </summary>
    public List<Image> Slides { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of videos.
    /// </summary>
    public List<Video> Videos { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of music tracks.
    /// </summary>
    public List<Music> Music { get; set; } = [];
}
