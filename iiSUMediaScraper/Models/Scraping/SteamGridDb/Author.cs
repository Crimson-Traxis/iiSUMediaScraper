using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.SteamGridDb;

public class Author
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("steam64")]
    public string Steam64 { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}
