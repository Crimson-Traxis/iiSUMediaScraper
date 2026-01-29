using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class Screenshot
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }
}