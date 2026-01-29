using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class RegionDefinition
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; }

    [JsonPropertyName("releases")]
    public IEnumerable<Release> Releases { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }
}