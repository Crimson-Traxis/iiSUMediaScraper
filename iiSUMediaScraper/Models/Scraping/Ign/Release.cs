using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class Release
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("estimatedDate")]
    public bool EstimatedDate { get; set; }

    [JsonPropertyName("timeframeYear")]
    public string TimeframeYear { get; set; }

    [JsonPropertyName("platformAttributes")]
    public IEnumerable<PlatformAttribute> PlatformAttributes { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }
}
