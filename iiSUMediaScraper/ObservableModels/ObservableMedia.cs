using iiSUMediaScraper.Models;

namespace iiSUMediaScraper.ObservableModels;

public class ObservableMedia : BaseObservableModel<Media>
{
    public ObservableMedia(Media baseModel) : base(baseModel)
    {
    }

    public string? Url
    {
        get => _baseModel.Url;
        set => SetProperty(_baseModel.Url, value, _baseModel, (o, v) => o.Url = v);
    }

    public string? Extension
    {
        get => _baseModel.Extension;
        set => SetProperty(_baseModel.Extension, value, _baseModel, (o, v) => o.Extension = v);
    }

    public byte[] Bytes
    {
        get => _baseModel.Bytes;
        set => SetProperty(_baseModel.Bytes, value, _baseModel, (o, v) => o.Bytes = v);
    }

    public SourceFlag Source
    {
        get => _baseModel.Source;
        set => SetProperty(_baseModel.Source, value, _baseModel, (o, v) => o.Source = v);
    }
}
