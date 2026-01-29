using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservablePlatformConfiguration : BaseObservableModel<PlatformConfiguration>
{
    public ObservablePlatformConfiguration(PlatformConfiguration baseModel) : base(baseModel)
    {
    }

    public string Code
    {
        get => _baseModel.Code;
        set => SetProperty(_baseModel.Code, value, _baseModel, (o, v) => o.Code = v);
    }

    public string Name
    {
        get => _baseModel.Name;
        set => SetProperty(_baseModel.Name, value, _baseModel, (o, v) => o.Name = v);
    }

    public string IconPath
    {
        get => _baseModel.IconPath;
        set => SetProperty(_baseModel.IconPath, value, _baseModel, (o, v) => o.IconPath = v);
    }
}
