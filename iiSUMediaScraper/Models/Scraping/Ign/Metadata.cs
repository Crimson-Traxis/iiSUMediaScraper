using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class Metadata
{
    [JsonPropertyName("names")]
    public NameDefinition Names { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }
}
