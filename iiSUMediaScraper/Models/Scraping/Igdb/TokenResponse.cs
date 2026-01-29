using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string Type { get; set; }

    public DateTime ExpiresOn { get; set; }
}
