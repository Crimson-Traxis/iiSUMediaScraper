namespace iiSUMediaScraper.Models;

public class MediaContext
{
    public List<Image> Icons { get; set; } = [];

    public List<Image> Logos { get; set; } = [];

    public List<Image> Titles { get; set; } = [];

    public List<Media> Heros { get; set; } = [];

    public List<Media> Slides { get; set; } = [];
}
