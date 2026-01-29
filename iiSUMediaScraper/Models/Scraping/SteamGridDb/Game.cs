using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.SteamGridDb;

public class Game
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("verified")]
    public bool IsVerified { get; set; }

    [JsonPropertyName("types")]
    public IEnumerable<string> Types { get; set; }

    public IEnumerable<Hero> Heros { get; set; } = [];

    public IEnumerable<Grid> Grids { get; set; } = [];

    public IEnumerable<Logo> Logos { get; set; } = [];
}
