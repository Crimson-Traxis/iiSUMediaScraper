using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class Genre
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }
}