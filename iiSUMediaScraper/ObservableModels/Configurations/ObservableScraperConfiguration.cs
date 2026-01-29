using iiSUMediaScraper.Models;
using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservableScraperConfiguration : BaseObservableModel<ScraperConfiguration>
{
    public ObservableScraperConfiguration(ScraperConfiguration baseModel) : base(baseModel)
    {
    }

    public SourceFlag Source
    {
        get => _baseModel.Source;
        set => SetProperty(_baseModel.Source, value, _baseModel, (o, v) => o.Source = v);
    }

    public bool IsFetch
    {
        get => _baseModel.IsFetch;
        set => SetProperty(_baseModel.IsFetch, value, _baseModel, (o, v) => o.IsFetch = v);
    }

    public bool IsFetchIcons
    {
        get => _baseModel.IsFetchIcons;
        set => SetProperty(_baseModel.IsFetchIcons, value, _baseModel, (o, v) => o.IsFetchIcons = v);
    }

    public bool IsFetchLogos
    {
        get => _baseModel.IsFetchLogos;
        set => SetProperty(_baseModel.IsFetchLogos, value, _baseModel, (o, v) => o.IsFetchLogos = v);
    }

    public bool IsFetchTitles
    {
        get => _baseModel.IsFetchTitles;
        set => SetProperty(_baseModel.IsFetchTitles, value, _baseModel, (o, v) => o.IsFetchTitles = v);
    }

    public bool IsFetchHeros
    {
        get => _baseModel.IsFetchHeros;
        set => SetProperty(_baseModel.IsFetchHeros, value, _baseModel, (o, v) => o.IsFetchHeros = v);
    }

    public bool IsFetchSlides
    {
        get => _baseModel.IsFetchSlides;
        set => SetProperty(_baseModel.IsFetchSlides, value, _baseModel, (o, v) => o.IsFetchSlides = v);
    }

    public bool IsFetchVideos
    {
        get => _baseModel.IsFetchVideos;
        set => SetProperty(_baseModel.IsFetchVideos, value, _baseModel, (o, v) => o.IsFetchVideos = v);
    }

    public bool IsFetchIconsIfNoneFound
    {
        get => _baseModel.IsFetchIconsIfNoneFound;
        set => SetProperty(_baseModel.IsFetchIconsIfNoneFound, value, _baseModel, (o, v) => o.IsFetchIconsIfNoneFound = v);
    }

    public bool IsFetchLogosIfNoneFound
    {
        get => _baseModel.IsFetchLogosIfNoneFound;
        set => SetProperty(_baseModel.IsFetchLogosIfNoneFound, value, _baseModel, (o, v) => o.IsFetchLogosIfNoneFound = v);
    }

    public bool IsFetchTitlesIfNoneFound
    {
        get => _baseModel.IsFetchTitlesIfNoneFound;
        set => SetProperty(_baseModel.IsFetchTitlesIfNoneFound, value, _baseModel, (o, v) => o.IsFetchTitlesIfNoneFound = v);
    }

    public bool IsFetchHerosIfNoneFound
    {
        get => _baseModel.IsFetchHerosIfNoneFound;
        set => SetProperty(_baseModel.IsFetchHerosIfNoneFound, value, _baseModel, (o, v) => o.IsFetchHerosIfNoneFound = v);
    }

    public bool IsFetchSlidesIfNoneFound
    {
        get => _baseModel.IsFetchSlidesIfNoneFound;
        set => SetProperty(_baseModel.IsFetchSlidesIfNoneFound, value, _baseModel, (o, v) => o.IsFetchSlidesIfNoneFound = v);
    }

    public bool IsAllowTitleAsIconWhenNoIconFound
    {
        get => _baseModel.IsAllowTitleAsIconWhenNoIconFound;
        set => SetProperty(_baseModel.IsAllowTitleAsIconWhenNoIconFound, value, _baseModel, (o, v) => o.IsAllowTitleAsIconWhenNoIconFound = v);
    }

    public bool IsUseSquareIconPriority
    {
        get => _baseModel.IsUseSquareIconPriority;
        set => SetProperty(_baseModel.IsUseSquareIconPriority, value, _baseModel, (o, v) => o.IsUseSquareIconPriority = v);
    }

    public int IconPriority
    {
        get => _baseModel.IconPriority;
        set => SetProperty(_baseModel.IconPriority, value, _baseModel, (o, v) => o.IconPriority = v);
    }

    public int LogoPriority
    {
        get => _baseModel.LogoPriority;
        set => SetProperty(_baseModel.LogoPriority, value, _baseModel, (o, v) => o.LogoPriority = v);
    }

    public int TitlePriority
    {
        get => _baseModel.TitlePriority;
        set => SetProperty(_baseModel.TitlePriority, value, _baseModel, (o, v) => o.TitlePriority = v);
    }

    public int HeroPriority
    {
        get => _baseModel.HeroPriority;
        set => SetProperty(_baseModel.HeroPriority, value, _baseModel, (o, v) => o.HeroPriority = v);
    }

    public int SlidePriority
    {
        get => _baseModel.SlidePriority;
        set => SetProperty(_baseModel.SlidePriority, value, _baseModel, (o, v) => o.SlidePriority = v);
    }

    public List<string> IconStyles
    {
        get => _baseModel.IconStyles;
        set => SetProperty(_baseModel.IconStyles, value, _baseModel, (o, v) => o.IconStyles = v);
    }

    public List<string> LogoStyles
    {
        get => _baseModel.LogoStyles;
        set => SetProperty(_baseModel.LogoStyles, value, _baseModel, (o, v) => o.LogoStyles = v);
    }

    public List<string> TitleStyles
    {
        get => _baseModel.TitleStyles;
        set => SetProperty(_baseModel.TitleStyles, value, _baseModel, (o, v) => o.TitleStyles = v);
    }

    public List<string> HeroStyles
    {
        get => _baseModel.HeroStyles;
        set => SetProperty(_baseModel.HeroStyles, value, _baseModel, (o, v) => o.HeroStyles = v);
    }

    public List<string> SlideStyles
    {
        get => _baseModel.SlideStyles;
        set => SetProperty(_baseModel.SlideStyles, value, _baseModel, (o, v) => o.SlideStyles = v);
    }

    public List<PlatformTranslationConfiguration> PlatformTranslationConfigurations
    {
        get => _baseModel.PlatformTranslationConfigurations;
        set => SetProperty(_baseModel.PlatformTranslationConfigurations, value, _baseModel, (o, v) => o.PlatformTranslationConfigurations = v);
    }
}
