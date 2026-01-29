using iiSUMediaScraper.Models.Scraping.Igdb.Converters;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb;

public class InvolvedCompany : IHasId
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}