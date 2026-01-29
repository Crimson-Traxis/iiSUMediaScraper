using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class NameDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("alt")]
    public IEnumerable<string> Alternates { get; set; } = [];

    [JsonPropertyName("short")]
    public string Short { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }
}
