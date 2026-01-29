using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservableGameIconOverlayConfiguration : BaseObservableModel<GameIconOverlayConfiguration>
{
    public ObservableGameIconOverlayConfiguration(GameIconOverlayConfiguration baseModel) : base(baseModel)
    {
    }

    public string Platform
    {
        get => _baseModel.Platform;
        set => SetProperty(_baseModel.Platform, value, _baseModel, (o, v) => o.Platform = v);
    }

    public string? Path
    {
        get => _baseModel.Path;
        set => SetProperty(_baseModel.Path, value, _baseModel, (o, v) => o.Path = v);
    }
}
