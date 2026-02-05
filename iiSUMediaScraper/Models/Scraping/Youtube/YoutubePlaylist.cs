using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Youtube;

public class YoutubePlaylist
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("_type")]
    public string? Type { get; set; }

    [JsonPropertyName("ie_key")]
    public string? IeKey { get; set; }
}
