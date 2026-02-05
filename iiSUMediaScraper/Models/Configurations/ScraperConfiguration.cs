namespace iiSUMediaScraper.Models.Configurations;

/// <summary>
/// Configuration for a media scraper source (IGDB, SteamGridDB, IGN, etc.).
/// </summary>
public class ScraperConfiguration
{
    /// <summary>
    /// Gets or sets the source type for this scraper.
    /// </summary>
    public SourceFlag Source { get; set; }

    /// <summary>
    /// Gets or sets whether this scraper is enabled.
    /// </summary>
    public bool IsFetch { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch icon images.
    /// </summary>
    public bool IsFetchIcons { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch logo images.
    /// </summary>
    public bool IsFetchLogos { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch title images.
    /// </summary>
    public bool IsFetchTitles { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch hero images.
    /// </summary>
    public bool IsFetchHeros { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch slide images.
    /// </summary>
    public bool IsFetchSlides { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch videos.
    /// </summary>
    public bool IsFetchVideos { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch icons only if none were found from other sources.
    /// </summary>
    public bool IsFetchIconsIfNoneFound { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch logos only if none were found from other sources.
    /// </summary>
    public bool IsFetchLogosIfNoneFound { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch titles only if none were found from other sources.
    /// </summary>
    public bool IsFetchTitlesIfNoneFound { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch heroes only if none were found from other sources.
    /// </summary>
    public bool IsFetchHerosIfNoneFound { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch slides only if none were found from other sources.
    /// </summary>
    public bool IsFetchSlidesIfNoneFound { get; set; }

    /// <summary>
    /// Gets or sets whether to allow title images as icons when no icon is found.
    /// </summary>
    public bool IsAllowTitleAsIconWhenNoIconFound { get; set; }

    /// <summary>
    /// Gets or sets whether to prioritize square icons.
    /// </summary>
    public bool IsUseSquareIconPriority { get; set; }

    /// <summary>
    /// Gets or sets the priority for icon images (lower is higher priority).
    /// </summary>
    public int IconPriority { get; set; }

    /// <summary>
    /// Gets or sets the priority for logo images (lower is higher priority).
    /// </summary>
    public int LogoPriority { get; set; }

    /// <summary>
    /// Gets or sets the priority for title images (lower is higher priority).
    /// </summary>
    public int TitlePriority { get; set; }

    /// <summary>
    /// Gets or sets the priority for hero images (lower is higher priority).
    /// </summary>
    public int HeroPriority { get; set; }

    /// <summary>
    /// Gets or sets the priority for slide images (lower is higher priority).
    /// </summary>
    public int SlidePriority { get; set; }

    /// <summary>
    /// Gets or sets the fetch limit for icon images.
    /// </summary>
    public int? IconFetchLimit { get; set; }

    /// <summary>
    /// Gets or sets the fetch limit for logo images.
    /// </summary>
    public int? LogoFetchLimit { get; set; }

    /// <summary>
    /// Gets or sets the fetch limit for title images.
    /// </summary>
    public int? TitleFetchLimit { get; set; }

    /// <summary>
    /// Gets or sets the fetch limit for hero images.
    /// </summary>
    public int? HeroFetchLimit { get; set; }

    /// <summary>
    /// Gets or sets the fetch limit for slide images.
    /// </summary>
    public int? SlideFetchLimit { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch music.
    /// </summary>
    public bool IsFetchMusic { get; set; }

    /// <summary>
    /// Gets or sets the list of icon styles to fetch.
    /// </summary>
    public List<string> IconStyles { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of logo styles to fetch.
    /// </summary>
    public List<string> LogoStyles { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of title styles to fetch.
    /// </summary>
    public List<string> TitleStyles { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of hero styles to fetch.
    /// </summary>
    public List<string> HeroStyles { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of slide styles to fetch.
    /// </summary>
    public List<string> SlideStyles { get; set; } = [];

    /// <summary>
    /// Gets or sets the platform translation configurations for this scraper.
    /// </summary>
    public List<PlatformTranslationConfiguration> PlatformTranslationConfigurations { get; set; } = [];
}
