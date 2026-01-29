using iiSUMediaScraper.Models.Scraping.Igdb.Converters;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb;

public class ReleaseDate : IHasId
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}