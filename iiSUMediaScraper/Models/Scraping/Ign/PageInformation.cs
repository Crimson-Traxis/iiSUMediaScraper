using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class PageInformation
{
    [JsonPropertyName("hasNext")]
    public bool HasNext { get; set; }

    [JsonPropertyName("nextCursor")]
    public int? NextCursor { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("__typename")]
    public string TypeName { get; set; }
}
