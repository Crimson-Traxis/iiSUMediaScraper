using System.Text.Json;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models;

/// <summary>
/// JSON converter for serializing and deserializing SourceFlag enum values.
/// </summary>
public class SourceFlagConverter : JsonConverter<SourceFlag>
{
    /// <summary>
    /// Reads a SourceFlag value from JSON.
    /// </summary>
    public override SourceFlag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return value switch
        {
            _ => Enum.Parse<SourceFlag>(value, ignoreCase: true)
        };
    }

    /// <summary>
    /// Writes a SourceFlag value to JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, SourceFlag value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            _ => value.ToString()
        };
        writer.WriteStringValue(stringValue);
    }

    /// <summary>
    /// Reads a SourceFlag value used as a JSON property name.
    /// </summary>
    public override SourceFlag ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return value switch
        {
            _ => Enum.Parse<SourceFlag>(value, ignoreCase: true)
        };
    }

    /// <summary>
    /// Writes a SourceFlag value as a JSON property name.
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, SourceFlag value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            _ => value.ToString()
        };
        writer.WritePropertyName(stringValue);
    }
}

/// <summary>
/// Specifies the source from which media was obtained.
/// </summary>
[JsonConverter(typeof(SourceFlagConverter))]
public enum SourceFlag
{
    /// <summary>
    /// IGDB (Internet Game Database) source.
    /// </summary>
    Igdb,

    /// <summary>
    /// IGN source.
    /// </summary>
    Ign,

    /// <summary>
    /// SteamGridDB source.
    /// </summary>
    SteamGridDb,

    /// <summary>
    /// YouTube source.
    /// </summary>
    Youtube,

    /// <summary>
    /// User-pasted content.
    /// </summary>
    Paste,

    /// <summary>
    /// Local file source.
    /// </summary>
    Local,

    /// <summary>
    /// Previous media file.
    /// </summary>
    Previous
}
