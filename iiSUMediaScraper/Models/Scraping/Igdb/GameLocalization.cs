using iiSUMediaScraper.Models.Scraping.Igdb.Converters;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb;

public class GameLocalization : IHasId
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}