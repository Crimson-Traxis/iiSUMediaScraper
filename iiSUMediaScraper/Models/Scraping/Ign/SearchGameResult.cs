using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class SearchGameResult
{
    [JsonPropertyName("objects")]
    public IEnumerable<Game> Games { get; set; }

    [JsonPropertyName("pageInfo")]
    public PageInformation PageInformation { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }
}
