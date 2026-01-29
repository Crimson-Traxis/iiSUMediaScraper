using iiSUMediaScraper.Models.Scraping.Igdb.Converters;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb;

public class Collection : IHasId
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}