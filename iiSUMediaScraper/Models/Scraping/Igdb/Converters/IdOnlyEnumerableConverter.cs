using System.Text.Json;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb.Converters;

public class IdOnlyEnumerableConverter<T> : JsonConverter<IEnumerable<T>> where T : IHasId, new()
{
    public override IEnumerable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        List<T> list = [];

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return list;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                list.Add(new T { Id = reader.GetInt32() });
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                list.Add(JsonSerializer.Deserialize<T>(ref reader, options));
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (T item in value)
        {
            writer.WriteNumberValue(item.Id);
        }
        writer.WriteEndArray();
    }
}