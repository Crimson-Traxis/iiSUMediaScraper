using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class Game
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("wikiSlug")]
    public string WikiSlug { get; set; }

    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; }

    [JsonPropertyName("primaryImage")]
    public Screenshot PrimaryImage { get; set; }

    public Screenshot SecondaryImage { get; set; }

    [JsonPropertyName("producers")]
    public IEnumerable<Producer> Producers { get; set; }

    [JsonPropertyName("objectRegions")]
    public IEnumerable<RegionDefinition> Regions { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }

    [JsonPropertyName("features")]
    public IEnumerable<Feature> Features { get; set; }

    [JsonPropertyName("franchises")]
    public IEnumerable<Franchise> Franchises { get; set; }

    [JsonPropertyName("genres")]
    public IEnumerable<Genre> Genres { get; set; }

    [JsonPropertyName("publishers")]
    public IEnumerable<Publisher> Publishers { get; set; }

    public IEnumerable<Screenshot> Images { get; set; }
}
