using System.Text.Json;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb.Converters;

public class IdOnlyConverter<T> : JsonConverter<T> where T : IHasId, new()
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return new T { Id = reader.GetInt32() };
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Id);
    }
}