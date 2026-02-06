using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models.Configurations;
using Microsoft.Extensions.Logging;
using System.IO;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Manages application configuration including loading, saving, and initializing overlays and icons.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly string _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(iiSUMediaScraper));
    private readonly string _baseFolder = AppDomain.CurrentDomain.BaseDirectory;
    private readonly string _file = $"{nameof(Configuration)}.json";
    private readonly string _overlayFolder = $"Overlays";
    private readonly string _iconsFolder = $"Icons";
    private readonly string _toolsFolder = $"Tools";

    /// <summary>
    /// Initializes a new instance of the ConfigurationService.
    /// </summary>
    /// <param name="fileService">The file service for reading and writing files.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public ConfigurationService(IFileService fileService, ILogger<ConfigurationService> logger)
    {
        FileService = fileService;
        Logger = logger;
    }

    /// <summary>
    /// Loads platform overlay images from the Overlays directory and maps them to platforms.
    /// Copies overlay files from the application directory to the user's documents folder.
    /// </summary>
    private Task LoadOverlays()
    {
        return Task.Run(async () =>
        {
            try
            {
                if (Configuration != null)
                {
                    var iconOverlays = new List<GameIconOverlayConfiguration>(Configuration.GameIconOverlayConfigurations);

                    foreach (var icon in iconOverlays.ToList())
                    {
                        if (!string.IsNullOrWhiteSpace(icon.Path) && !await FileService.FileExists(icon.Path))
                        {
                            iconOverlays.Remove(icon);
                        }
                    }

                    foreach (string png in Directory.EnumerateFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _overlayFolder), "*.png", new EnumerationOptions() { RecurseSubdirectories = true }))
                    {
                        string name = Path.GetFileNameWithoutExtension(png).Trim();

                        string newPath = png.Replace(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _overlayFolder), Path.Combine(_folder, _overlayFolder));

                        string? newDirectory = Path.GetDirectoryName(newPath);

                        if (!string.IsNullOrEmpty(newDirectory))
                        {
                            if (!Directory.Exists(newDirectory))
                            {
                                Directory.CreateDirectory(newDirectory);
                            }

                            if (File.Exists(newPath))
                            {
                                File.Delete(newPath);
                            }

                            File.Copy(png, newPath);

                            if (!string.IsNullOrWhiteSpace(newPath))
                            {
                                switch (Path.GetFileNameWithoutExtension(newPath))
                                {
                                    case "Xbox":
                                        if (!iconOverlays.Any(c => c.Platform == "Xbox"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "Xbox", Path = newPath });
                                        }
                                        break;
                                    case "Xbox360":
                                        if (!iconOverlays.Any(c => c.Platform == "Xbox360"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "Xbox360", Path = newPath });
                                        }
                                        break;
                                    case "3DS":
                                        if (!iconOverlays.Any(c => c.Platform == "3DS"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "3DS", Path = newPath });
                                        }
                                        break;
                                    case "DS":
                                        if (!iconOverlays.Any(c => c.Platform == "NDS"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "NDS", Path = newPath });
                                        }
                                        break;
                                    case "e-shop":
                                        if (!iconOverlays.Any(c => c.Platform == "EShop"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "EShop", Path = newPath });
                                        }
                                        break;
                                    case "Game_Boy":
                                        if (!iconOverlays.Any(c => c.Platform == "GB"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "GB", Path = newPath });
                                        }
                                        break;
                                    case "Game_Boy_Advance":
                                        if (!iconOverlays.Any(c => c.Platform == "GBA"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "GBA", Path = newPath });
                                        }
                                        break;
                                    case "Game_Boy_Color":
                                        if (!iconOverlays.Any(c => c.Platform == "GBC"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "GBC", Path = newPath });
                                        }
                                        break;
                                    case "Gamecube":
                                        if (!iconOverlays.Any(c => c.Platform == "GC"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "GC", Path = newPath });
                                        }
                                        break;
                                    case "N64":
                                        if (!iconOverlays.Any(c => c.Platform == "N64"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "N64", Path = newPath });
                                        }
                                        break;
                                    case "NES":
                                        if (!iconOverlays.Any(c => c.Platform == "NES"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "NES", Path = newPath });
                                        }
                                        break;
                                    case "SNES":
                                        if (!iconOverlays.Any(c => c.Platform == "SNES"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "SNES", Path = newPath });
                                        }
                                        break;
                                    case "Switch":
                                        if (!iconOverlays.Any(c => c.Platform == "Switch"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "Switch", Path = newPath });
                                        }
                                        break;
                                    case "Wii":
                                        if (!iconOverlays.Any(c => c.Platform == "Wii"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "Wii", Path = newPath });
                                        }
                                        break;
                                    case "Wii_U":
                                        if (!iconOverlays.Any(c => c.Platform == "WiiU"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "WiiU", Path = newPath });
                                        }
                                        break;
                                    case "Android":
                                        if (!iconOverlays.Any(c => c.Platform == "Android"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "Android", Path = newPath });
                                        }
                                        break;
                                    case "Atari2600":
                                        if (!iconOverlays.Any(c => c.Platform == "Atari2600"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "Atari2600", Path = newPath });
                                        }
                                        break;
                                    case "GOG":
                                        if (!iconOverlays.Any(c => c.Platform == "GOG"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "GOG", Path = newPath });
                                        }
                                        break;
                                    case "Neo_Geo_Pocket_Color":
                                        if (!iconOverlays.Any(c => c.Platform == "NeoGeoPocketColor"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "NeoGeoPocketColor", Path = newPath });
                                        }
                                        break;
                                    case "PC":
                                        if (!iconOverlays.Any(c => c.Platform == "PC"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "PC", Path = newPath });
                                        }
                                        break;
                                    case "Steam":
                                        if (!iconOverlays.Any(c => c.Platform == "Steam"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "Steam", Path = newPath });
                                        }
                                        break;
                                    case "Dreamcast":
                                        if (!iconOverlays.Any(c => c.Platform == "DreamCast"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "DreamCast", Path = newPath });
                                        }
                                        break;
                                    case "Game_Gear":
                                        if (!iconOverlays.Any(c => c.Platform == "GameGear"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "GameGear", Path = newPath });
                                        }
                                        break;
                                    case "Megadrive-Genesis":
                                        if (!iconOverlays.Any(c => c.Platform == "MegadriveGenesis"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "MegadriveGenesis", Path = newPath });
                                        }
                                        break;
                                    case "Saturn":
                                        if (!iconOverlays.Any(c => c.Platform == "Saturn"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "Saturn", Path = newPath });
                                        }
                                        break;
                                    case "PS_Vita":
                                        if (!iconOverlays.Any(c => c.Platform == "PSVita"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "PSVita", Path = newPath });
                                        }
                                        break;
                                    case "PS2":
                                        if (!iconOverlays.Any(c => c.Platform == "PS2"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "PS2", Path = newPath });
                                        }
                                        break;
                                    case "PS3":
                                        if (!iconOverlays.Any(c => c.Platform == "PS3"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "PS3", Path = newPath });
                                        }
                                        break;
                                    case "PS4":
                                        if (!iconOverlays.Any(c => c.Platform == "PS4"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "PS4", Path = newPath });
                                        }
                                        break;
                                    case "PSP":
                                        if (!iconOverlays.Any(c => c.Platform == "PSP"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "PSP", Path = newPath });
                                        }
                                        break;
                                    case "PSX":
                                        if (!iconOverlays.Any(c => c.Platform == "PSX"))
                                        {
                                            iconOverlays.Add(new GameIconOverlayConfiguration { Platform = "PSX", Path = newPath });
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load overlays");
            }
        });
    }

    /// <summary>
    /// Loads platform icon images from the Icons directory and maps them to platforms.
    /// Copies icon files from the application directory to the user's documents folder.
    /// </summary>
    private Task LoadIcons()
    {
        return Task.Run(async () =>
        {
            try
            {
                if (Configuration != null)
                {
                    var platformIcons = new List<PlatformIconConfiguration>(Configuration.PlatformIconConfigurations);

                    foreach (var icon in platformIcons.ToList())
                    {
                        if (!string.IsNullOrWhiteSpace(icon.Path) && !await FileService.FileExists(icon.Path))
                        {
                            platformIcons.Remove(icon);
                        }
                    }

                    foreach (string png in Directory.EnumerateFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _iconsFolder), "*.png", new EnumerationOptions() { RecurseSubdirectories = true }))
                    {
                        string name = Path.GetFileNameWithoutExtension(png).Trim();

                        string newPath = png.Replace(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _iconsFolder), Path.Combine(_folder, _iconsFolder));

                        string? newDirectory = Path.GetDirectoryName(newPath);

                        if (!string.IsNullOrEmpty(newDirectory))
                        {
                            if (!Directory.Exists(newDirectory))
                            {
                                Directory.CreateDirectory(newDirectory);
                            }

                            if (File.Exists(newPath))
                            {
                                File.Delete(newPath);
                            }

                            File.Copy(png, newPath);

                            if (!string.IsNullOrWhiteSpace(newPath))
                            {
                                switch (Path.GetFileNameWithoutExtension(newPath))
                                {
                                    case "Nintendo 3DS":
                                        if (!platformIcons.Any(c => c.Platform == "3DS"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "3DS", Path = newPath });
                                        }
                                        break;
                                    case "Nintendo DS":
                                        if (!platformIcons.Any(c => c.Platform == "NDS"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "NDS", Path = newPath });
                                        }
                                        break;
                                    case "Nintendo eShop":
                                        if (!platformIcons.Any(c => c.Platform == "EShop"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "EShop", Path = newPath });
                                        }
                                        break;
                                    case "Gameboy":
                                        if (!platformIcons.Any(c => c.Platform == "GB"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "GB", Path = newPath });
                                        }
                                        break;
                                    case "Gameboy Advance":
                                        if (!platformIcons.Any(c => c.Platform == "GBA"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "GBA", Path = newPath });
                                        }
                                        break;
                                    case "Gameboy Color":
                                        if (!platformIcons.Any(c => c.Platform == "GBC"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "GBC", Path = newPath });
                                        }
                                        break;
                                    case "Nintendo GameCube":
                                        if (!platformIcons.Any(c => c.Platform == "GC"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "GC", Path = newPath });
                                        }
                                        break;
                                    case "Nintendo 64":
                                        if (!platformIcons.Any(c => c.Platform == "N64"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "N64", Path = newPath });
                                        }
                                        break;
                                    case "Famicom":
                                        if (!platformIcons.Any(c => c.Platform == "NES"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "NES", Path = newPath });
                                        }
                                        break;
                                    case "Super Famicom":
                                        if (!platformIcons.Any(c => c.Platform == "SNES"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "SNES", Path = newPath });
                                        }
                                        break;
                                    case "Nintendo Switch":
                                        if (!platformIcons.Any(c => c.Platform == "Switch"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "Switch", Path = newPath });
                                        }
                                        break;
                                    case "Nintendo Wii":
                                        if (!platformIcons.Any(c => c.Platform == "Wii"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "Wii", Path = newPath });
                                        }
                                        break;
                                    case "Nintendo Wii U":
                                        if (!platformIcons.Any(c => c.Platform == "WiiU"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "WiiU", Path = newPath });
                                        }
                                        break;
                                    case "Android":
                                        if (!platformIcons.Any(c => c.Platform == "Android"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "Android", Path = newPath });
                                        }
                                        break;
                                    case "Atari2600":
                                        if (!platformIcons.Any(c => c.Platform == "Atari2600"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "Atari2600", Path = newPath });
                                        }
                                        break;
                                    case "GOG":
                                        if (!platformIcons.Any(c => c.Platform == "GOG"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "GOG", Path = newPath });
                                        }
                                        break;
                                    case "SNK Neo Geo Pocket Color;":
                                        if (!platformIcons.Any(c => c.Platform == "NeoGeoPocketColor"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "NeoGeoPocketColor", Path = newPath });
                                        }
                                        break;
                                    case "PC":
                                        if (!platformIcons.Any(c => c.Platform == "PC"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "PC", Path = newPath });
                                        }
                                        break;
                                    case "Steam":
                                        if (!platformIcons.Any(c => c.Platform == "Steam"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "Steam", Path = newPath });
                                        }
                                        break;
                                    case "Sega Dreamcast":
                                        if (!platformIcons.Any(c => c.Platform == "DreamCast"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "DreamCast", Path = newPath });
                                        }
                                        break;
                                    case "Sega Game Gear":
                                        if (!platformIcons.Any(c => c.Platform == "GameGear"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "GameGear", Path = newPath });
                                        }
                                        break;
                                    case "Sega Mega Drive":
                                        if (!platformIcons.Any(c => c.Platform == "MegadriveGenesis"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "MegadriveGenesis", Path = newPath });
                                        }
                                        break;
                                    case "Sega Saturn":
                                        if (!platformIcons.Any(c => c.Platform == "Saturn"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "Saturn", Path = newPath });
                                        }
                                        break;
                                    case "PlayStation VITA":
                                        if (!platformIcons.Any(c => c.Platform == "PSVita"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "PSVita", Path = newPath });
                                        }
                                        break;
                                    case "PlayStation 2":
                                        if (!platformIcons.Any(c => c.Platform == "PS2"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "PS2", Path = newPath });
                                        }
                                        break;
                                    case "PlayStation 3":
                                        if (!platformIcons.Any(c => c.Platform == "PS3"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "PS3", Path = newPath });
                                        }
                                        break;
                                    case "PlayStation 4":
                                        if (!platformIcons.Any(c => c.Platform == "PS4"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "PS4", Path = newPath });
                                        }
                                        break;
                                    case "PlayStation Portable":
                                        if (!platformIcons.Any(c => c.Platform == "PSP"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "PSP", Path = newPath });
                                        }
                                        break;
                                    case "PlayStation":
                                        if (!platformIcons.Any(c => c.Platform == "PSX"))
                                        {
                                            platformIcons.Add(new PlatformIconConfiguration { Platform = "PSX", Path = newPath });
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load icons");
            }
        });
    }

    /// <summary>
    /// Copies external tools (yt-dlp.exe, ffmpeg.exe, ffprobe.exe, ffplay.exe) from the application's Tools directory
    /// to the user's documents folder if they don't already exist. This allows users to update the executables independently.
    /// </summary>
    private Task CopyExternalTools()
    {
        return Task.Run(() =>
        {
            try
            {
                var toolsSourceFolder = Path.Combine(_baseFolder, _toolsFolder);
                var toolsDestFolder = Path.Combine(_folder, _toolsFolder);

                if (!Directory.Exists(toolsDestFolder))
                {
                    Directory.CreateDirectory(toolsDestFolder);
                }

                string[] tools = ["yt-dlp.exe", "ffmpeg.exe", "ffprobe.exe", "ffplay.exe"];

                foreach (var tool in tools)
                {
                    var sourcePath = Path.Combine(toolsSourceFolder, tool);
                    var destPath = Path.Combine(toolsDestFolder, tool);

                    if (File.Exists(sourcePath) && !File.Exists(destPath))
                    {
                        File.Copy(sourcePath, destPath);
                        Logger.LogDebug("Copied {Tool} to {Destination}", tool, destPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to copy external tools");
            }
        });
    }

    /// <summary>
    /// Loads the configuration from disk.
    /// First attempts to load from the user's Documents folder, then falls back to the application directory.
    /// Creates a default configuration if none exists, then loads overlays and icons.
    /// </summary>
    public async Task LoadConfiguration()
    {
        try
        {
            Configuration = await FileService.Read<Configuration>(_folder, _file);
            Logger.LogDebug("Configuration loaded from user folder: {Folder}", _folder);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load configuration from user folder: {Folder}", _folder);
        }

        try
        {
            Configuration ??= await FileService.Read<Configuration>(_baseFolder, _file);
            if (Configuration != null)
            {
                Logger.LogDebug("Configuration loaded from base folder: {Folder}", _baseFolder);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load configuration from base folder: {Folder}", _baseFolder);
        }

        if (Configuration == null)
        {
            Configuration = new Configuration();
            Logger.LogInformation("Created default configuration");
        }


        Configuration.PlatformConfigurations = [.. Configuration.PlatformConfigurations.OrderBy(p => p.Name)];
        Configuration.FolderConfigurations = [.. Configuration.FolderConfigurations.OrderBy(f => Configuration.PlatformConfigurations.FirstOrDefault(p => p.Code == f.Platform)?.Name)];
        Configuration.GameIconOverlayConfigurations = [.. Configuration.GameIconOverlayConfigurations.OrderBy(i => Configuration.PlatformConfigurations.FirstOrDefault(p => p.Code == i.Platform)?.Name)];
        Configuration.PlatformIconConfigurations = [.. Configuration.PlatformIconConfigurations.OrderBy(i => Configuration.PlatformConfigurations.FirstOrDefault(p => p.Code == i.Platform)?.Name)];

        await LoadOverlays();

        await LoadIcons();

        await CopyExternalTools();
    }

    /// <summary>
    /// Saves the current configuration to disk in the user's Documents folder.
    /// </summary>
    public async Task SaveConfiguration()
    {
        if (Configuration != null)
        {
            try
            {
                await FileService.Save(_folder, _file, Configuration);
                Logger.LogDebug("Configuration saved to: {Folder}", _folder);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save configuration to: {Folder}", _folder);
                throw;
            }
        }
    }

    /// <summary>
    /// Gets the file service for reading and writing files.
    /// </summary>
    protected IFileService FileService { get; private set; }

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }

    /// <summary>
    /// Gets or sets the current application configuration.
    /// </summary>
    public Configuration? Configuration { get; set; }

    /// <summary>
    /// Gets the folder path where external tools (yt-dlp.exe, ffmpeg.exe, ffprobe.exe, ffplay.exe) are located.
    /// </summary>
    public string ToolsFolder => Path.Combine(_folder, _toolsFolder);
}
