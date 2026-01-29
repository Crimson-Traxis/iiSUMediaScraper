using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.ObservableModels.Configurations;

public class ObservableUpscalerConfiguration : BaseObservableModel<UpscalerConfiguration>
{
    public ObservableUpscalerConfiguration(UpscalerConfiguration baseModel) : base(baseModel)
    {
    }

    public string Name
    {
        get => _baseModel.Name;
        set => SetProperty(_baseModel.Name, value, _baseModel, (o, v) => o.Name = v);
    }


    public int Seed
    {
        get => _baseModel.Seed;
        set => SetProperty(_baseModel.Seed, value, _baseModel, (o, v) => o.Seed = v);
    }

    public string ColorCorrection
    {
        get => _baseModel.ColorCorrection;
        set => SetProperty(_baseModel.ColorCorrection, value, _baseModel, (o, v) => o.ColorCorrection = v);
    }

    public double InputNoiseScale
    {
        get => _baseModel.InputNoiseScale;
        set => SetProperty(_baseModel.InputNoiseScale, value, _baseModel, (o, v) => o.InputNoiseScale = v);
    }

    public double LatentNoiseScale
    {
        get => _baseModel.LatentNoiseScale;
        set => SetProperty(_baseModel.LatentNoiseScale, value, _baseModel, (o, v) => o.LatentNoiseScale = v);
    }
}
