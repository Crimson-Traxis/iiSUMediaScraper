using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.SteamGridDb;

public class Response<T>
{
    [JsonPropertyName("success")]
    public bool IsSucess { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("data")]
    public IEnumerable<T> Data { get; set; }
}
