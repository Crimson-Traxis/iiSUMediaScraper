using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Ign;

public class Data<T>
{
    [JsonPropertyName("searchObjectsByName")]
    public T Result { get; set; }
}
