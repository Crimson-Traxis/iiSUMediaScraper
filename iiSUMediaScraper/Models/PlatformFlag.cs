using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models;

public static class PlatformFlagExtensions
{
    private static readonly PlatformFlag[] _allValues = Enum.GetValues<PlatformFlag>();

    public static string ToDisplayString(this PlatformFlag value)
    {
        return value switch
        {
            PlatformFlag.ThreeDS => "3DS",
            _ => value.ToString()
        };
    }

    public static IEnumerable<string> GetAllDisplayStrings()
    {
        return _allValues.Select(p => p.ToDisplayString());
    }

    public static Dictionary<PlatformFlag, string> GetAllWithDisplayStrings()
    {
        return _allValues.ToDictionary(p => p, p => p.ToDisplayString());
    }

    public static string GetDisplayName(this PlatformFlag value)
    {
        FieldInfo? field = typeof(PlatformFlag).GetField(value.ToString());
        DisplayNameAttribute? attribute = field?.GetCustomAttribute<DisplayNameAttribute>();
        return attribute?.Name ?? value.ToString();
    }

    public static string ToJsonString(this PlatformFlag value)
    {
        return value switch
        {
            PlatformFlag.ThreeDS => "3DS",
            _ => value.ToString()
        };
    }

    public static IEnumerable<(PlatformFlag Value, string DisplayName)> GetAll()
    {
        return _allValues
            .Select(p => (p, p.GetDisplayName()));
    }

    public static Dictionary<PlatformFlag, string> GetAllDisplayNames()
    {
        return _allValues
            .ToDictionary(p => p, p => p.GetDisplayName());
    }
}

public class PlatformFlagConverter : JsonConverter<PlatformFlag>
{
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

    public override void Write(Utf8JsonWriter writer, PlatformFlag value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            PlatformFlag.ThreeDS => "3DS",
            _ => value.ToString()
        };
        writer.WriteStringValue(stringValue);
    }

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

[AttributeUsage(AttributeTargets.Field)]
public class DisplayNameAttribute : Attribute
{
    public string Name { get; }

    public DisplayNameAttribute(string name)
    {
        Name = name;
    }
}

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
