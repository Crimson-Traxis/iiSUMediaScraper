using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservableSpecificGame : BaseObservableModel<SpecificGame>
{
    public ObservableSpecificGame(SpecificGame baseModel) : base(baseModel)
    {
    }

    public string Platform
    {
        get => _baseModel.Platform;
        set => SetProperty(_baseModel.Platform, value, _baseModel, (o, v) => o.Platform = v);
    }

    public string Name
    {
        get => _baseModel.Name;
        set => SetProperty(_baseModel.Name, value, _baseModel, (o, v) => o.Name = v);
    }
}
