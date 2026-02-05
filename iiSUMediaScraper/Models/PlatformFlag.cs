using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models;

/// <summary>
/// Extension methods for PlatformFlag enum.
/// </summary>
public static class PlatformFlagExtensions
{
    private static readonly PlatformFlag[] _allValues = Enum.GetValues<PlatformFlag>();

    /// <summary>
    /// Converts a PlatformFlag to its display string representation.
    /// </summary>
    /// <param name="value">The platform flag value.</param>
    /// <returns>The display string.</returns>
    public static string ToDisplayString(this PlatformFlag value)
    {
        return value switch
        {
            PlatformFlag.ThreeDS => "3DS",
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Gets all platform flag display strings.
    /// </summary>
    /// <returns>Collection of display strings for all platforms.</returns>
    public static IEnumerable<string> GetAllDisplayStrings()
    {
        return _allValues.Select(p => p.ToDisplayString());
    }

    /// <summary>
    /// Gets a dictionary mapping all platform flags to their display strings.
    /// </summary>
    /// <returns>Dictionary of platform flags to display strings.</returns>
    public static Dictionary<PlatformFlag, string> GetAllWithDisplayStrings()
    {
        return _allValues.ToDictionary(p => p, p => p.ToDisplayString());
    }

    /// <summary>
    /// Gets the friendly display name from the DisplayName attribute.
    /// </summary>
    /// <param name="value">The platform flag value.</param>
    /// <returns>The display name from attribute or the enum name.</returns>
    public static string GetDisplayName(this PlatformFlag value)
    {
        FieldInfo? field = typeof(PlatformFlag).GetField(value.ToString());
        DisplayNameAttribute? attribute = field?.GetCustomAttribute<DisplayNameAttribute>();
        return attribute?.Name ?? value.ToString();
    }

    /// <summary>
    /// Converts a PlatformFlag to its JSON string representation.
    /// </summary>
    /// <param name="value">The platform flag value.</param>
    /// <returns>The JSON string representation.</returns>
    public static string ToJsonString(this PlatformFlag value)
    {
        return value switch
        {
            PlatformFlag.ThreeDS => "3DS",
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Gets all platform flags with their display names.
    /// </summary>
    /// <returns>Collection of tuples containing platform flag and display name.</returns>
    public static IEnumerable<(PlatformFlag Value, string DisplayName)> GetAll()
    {
        return _allValues
            .Select(p => (p, p.GetDisplayName()));
    }

    /// <summary>
    /// Gets a dictionary mapping all platform flags to their display names.
    /// </summary>
    /// <returns>Dictionary of platform flags to display names.</returns>
    public static Dictionary<PlatformFlag, string> GetAllDisplayNames()
    {
        return _allValues
            .ToDictionary(p => p, p => p.GetDisplayName());
    }
}

/// <summary>
/// JSON converter for serializing and deserializing PlatformFlag enum values.
/// </summary>
public class PlatformFlagConverter : JsonConverter<PlatformFlag>
{
    /// <summary>
    /// Reads a PlatformFlag value from JSON.
    /// </summary>
    public override PlatformFlag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return value switch
        {
            "3DS" => PlatformFlag.ThreeDS,
            "ThreeDS" => PlatformFlag.ThreeDS,
            _ => Enum.Parse<PlatformFlag>(value, ignoreCase: true)
        };
    }

    /// <summary>
    /// Writes a PlatformFlag value to JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, PlatformFlag value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            PlatformFlag.ThreeDS => "3DS",
            _ => value.ToString()
        };
        writer.WriteStringValue(stringValue);
    }

    /// <summary>
    /// Reads a PlatformFlag value used as a JSON property name.
    /// </summary>
    public override PlatformFlag ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return value switch
        {
            "3DS" => PlatformFlag.ThreeDS,
            "ThreeDS" => PlatformFlag.ThreeDS,
            _ => Enum.Parse<PlatformFlag>(value, ignoreCase: true)
        };
    }

    /// <summary>
    /// Writes a PlatformFlag value as a JSON property name.
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, PlatformFlag value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            PlatformFlag.ThreeDS => "3DS",
            _ => value.ToString()
        };
        writer.WritePropertyName(stringValue);
    }
}

/// <summary>
/// Attribute for providing a friendly display name for enum values.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class DisplayNameAttribute : Attribute
{
    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of DisplayNameAttribute.
    /// </summary>
    /// <param name="name">The display name.</param>
    public DisplayNameAttribute(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Specifies the gaming platform for a game.
/// </summary>
[JsonConverter(typeof(PlatformFlagConverter))]
public enum PlatformFlag
{
    [DisplayName("None")]
    None,
    [DisplayName("Nintendo Entertainment System")]
    NES,
    [DisplayName("Super Nintendo Entertainment System")]
    SNES,
    [DisplayName("Nintendo 64")]
    N64,
    [DisplayName("Game Boy")]
    GB,
    [DisplayName("Game Boy Color")]
    GBC,
    [DisplayName("Game Boy Advance")]
    GBA,
    [DisplayName("GameCube")]
    GC,
    [DisplayName("eShop")]
    EShop,
    [DisplayName("Nintendo DS")]
    NDS,
    [DisplayName("Nintendo 3DS")]
    ThreeDS,
    [DisplayName("Wii")]
    Wii,
    [DisplayName("Wii U")]
    WiiU,
    [DisplayName("Nintendo Switch")]
    Switch,
    [DisplayName("PlayStation")]
    PSX,
    [DisplayName("PlayStation 2")]
    PS2,
    [DisplayName("PlayStation 3")]
    PS3,
    [DisplayName("PlayStation 4")]
    PS4,
    [DisplayName("PlayStation Portable")]
    PSP,
    [DisplayName("PlayStation Vita")]
    PSVita,
    [DisplayName("Xbox")]
    Xbox,
    [DisplayName("Xbox 360")]
    Xbox360,
    [DisplayName("Dreamcast")]
    DreamCast,
    [DisplayName("Game Gear")]
    GameGear,
    [DisplayName("Sega Genesis / Mega Drive")]
    MegadriveGenesis,
    [DisplayName("Sega Saturn")]
    Saturn,
    [DisplayName("Android")]
    Android,
    [DisplayName("Atari 2600")]
    Atari2600,
    [DisplayName("GOG")]
    GOG,
    [DisplayName("Neo Geo Pocket Color")]
    NeoGeoPocketColor,
    [DisplayName("PC")]
    PC,
    [DisplayName("Steam")]
    Steam
}
