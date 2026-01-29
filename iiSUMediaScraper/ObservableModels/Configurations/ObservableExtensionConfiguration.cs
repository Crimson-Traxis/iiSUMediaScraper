using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservableExtensionConfiguration : BaseObservableModel<ExtensionConfiguration>
{
    public ObservableExtensionConfiguration(ExtensionConfiguration baseModel) : base(baseModel)
    {
    }

    public string Platform
    {
        get => _baseModel.Platform;
        set => SetProperty(_baseModel.Platform, value, _baseModel, (o, v) => o.Platform = v);
    }

    public List<string> Extension
    {
        get => _baseModel.Extension;
        set => SetProperty(_baseModel.Extension, value, _baseModel, (o, v) => o.Extension = v);
    }
}
