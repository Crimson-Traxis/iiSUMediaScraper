using System.Text.Json;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models;

public class SourceFlagConverter : JsonConverter<SourceFlag>
{
    public override SourceFlag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return value switch
        {
            _ => Enum.Parse<SourceFlag>(value, ignoreCase: true)
        };
    }

    public override void Write(Utf8JsonWriter writer, SourceFlag value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            _ => value.ToString()
        };
        writer.WriteStringValue(stringValue);
    }

    public override SourceFlag ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return value switch
        {
            _ => Enum.Parse<SourceFlag>(value, ignoreCase: true)
        };
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, SourceFlag value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            _ => value.ToString()
        };
        writer.WritePropertyName(stringValue);
    }
}

[JsonConverter(typeof(SourceFlagConverter))]
public enum SourceFlag
{
    Igdb,
    Ign,
    SteamGridDb,
    Local
}
