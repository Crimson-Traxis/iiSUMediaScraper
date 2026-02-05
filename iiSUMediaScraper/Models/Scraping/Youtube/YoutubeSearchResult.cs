using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Youtube;

public class YoutubeSearchResult
{
    [JsonPropertyName("entries")]
    public List<YoutubePlaylist>? Entries { get; set; }
}
