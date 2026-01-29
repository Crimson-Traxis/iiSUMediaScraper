namespace iiSUMediaScraper.Models.Configurations;

public class ScraperConfiguration
{
    public SourceFlag Source { get; set; }

    public bool IsFetch { get; set; }

    public bool IsFetchIcons { get; set; }

    public bool IsFetchLogos { get; set; }

    public bool IsFetchTitles { get; set; }

    public bool IsFetchHeros { get; set; }

    public bool IsFetchSlides { get; set; }

    public bool IsFetchVideos { get; set; }

    public bool IsFetchIconsIfNoneFound { get; set; }

    public bool IsFetchLogosIfNoneFound { get; set; }

    public bool IsFetchTitlesIfNoneFound { get; set; }

    public bool IsFetchHerosIfNoneFound { get; set; }

    public bool IsFetchSlidesIfNoneFound { get; set; }

    public bool IsAllowTitleAsIconWhenNoIconFound { get; set; }

    public bool IsUseSquareIconPriority { get; set; }

    public int IconPriority { get; set; }

    public int LogoPriority { get; set; }

    public int TitlePriority { get; set; }

    public int HeroPriority { get; set; }

    public int SlidePriority { get; set; }

    public List<string> IconStyles { get; set; } = [];

    public List<string> LogoStyles { get; set; } = [];

    public List<string> TitleStyles { get; set; } = [];

    public List<string> HeroStyles { get; set; } = [];

    public List<string> SlideStyles { get; set; } = [];

    public List<PlatformTranslationConfiguration> PlatformTranslationConfigurations { get; set; } = [];
}
