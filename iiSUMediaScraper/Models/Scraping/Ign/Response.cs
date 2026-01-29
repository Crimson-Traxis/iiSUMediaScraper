using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class Response<T>
{
    [JsonPropertyName("data")]
    public Data<T> Data { get; set; }
}
