using iiSUMediaScraper.Models;

namespace iiSUMediaScraper.ObservableModels;

public class ObservableMediaContext : BaseObservableModel<MediaContext>
{
    public ObservableMediaContext(MediaContext baseModel) : base(baseModel)
    {
    }

    public List<Image> Icons
    {
        get => _baseModel.Icons;
        set => SetProperty(_baseModel.Icons, value, _baseModel, (o, v) => o.Icons = v);
    }

    public List<Image> Logos
    {
        get => _baseModel.Logos;
        set => SetProperty(_baseModel.Logos, value, _baseModel, (o, v) => o.Logos = v);
    }

    public List<Image> Titles
    {
        get => _baseModel.Titles;
        set => SetProperty(_baseModel.Titles, value, _baseModel, (o, v) => o.Titles = v);
    }

    public List<Media> Heros
    {
        get => _baseModel.Heros;
        set => SetProperty(_baseModel.Heros, value, _baseModel, (o, v) => o.Heros = v);
    }

    public List<Media> Slides
    {
        get => _baseModel.Slides;
        set => SetProperty(_baseModel.Slides, value, _baseModel, (o, v) => o.Slides = v);
    }
}
