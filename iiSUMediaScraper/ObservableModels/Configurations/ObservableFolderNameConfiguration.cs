using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservableFolderNameConfiguration : BaseObservableModel<FolderNameConfiguration>
{
    public ObservableFolderNameConfiguration(FolderNameConfiguration baseModel) : base(baseModel)
    {
    }

    public string Platform
    {
        get => _baseModel.Platform;
        set => SetProperty(_baseModel.Platform, value, _baseModel, (o, v) => o.Platform = v);
    }

    public string? Name
    {
        get => _baseModel.Name;
        set => SetProperty(_baseModel.Name, value, _baseModel, (o, v) => o.Name = v);
    }

    public bool IsAssetFolder
    {
        get => _baseModel.IsAssetFolder;
        set => SetProperty(_baseModel.IsAssetFolder, value, _baseModel, (o, v) => o.IsAssetFolder = v);
    }
}
