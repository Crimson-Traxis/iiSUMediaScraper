using iiSUMediaScraper.Models.Scraping.Igdb.Converters;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb;

public class AlternativeName : IHasId
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}