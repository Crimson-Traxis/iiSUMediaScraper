using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservablePlatformTranslationConfiguration : BaseObservableModel<PlatformTranslationConfiguration>
{
    public ObservablePlatformTranslationConfiguration(PlatformTranslationConfiguration baseModel) : base(baseModel)
    {
    }

    public string Platform
    {
        get => _baseModel.Platform;
        set => SetProperty(_baseModel.Platform, value, _baseModel, (o, v) => o.Platform = v);
    }

    public string Identifier
    {
        get => _baseModel.Identifier;
        set => SetProperty(_baseModel.Identifier, value, _baseModel, (o, v) => o.Identifier = v);
    }
}
