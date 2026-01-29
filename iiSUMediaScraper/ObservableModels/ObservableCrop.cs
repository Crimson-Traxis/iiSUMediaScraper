using iiSUMediaScraper.Models;

namespace iiSUMediaScraper.ObservableModels;

public class ObservableCrop : BaseObservableModel<Crop>
{
    public ObservableCrop(Crop baseModel) : base(baseModel)
    {
    }

    public int Top
    {
        get => BaseModel.Top;
        set => SetProperty(BaseModel.Top, value, BaseModel, (o, v) => o.Top = v);
    }

    public int Left
    {
        get => BaseModel.Left;
        set => SetProperty(BaseModel.Left, value, BaseModel, (o, v) => o.Left = v);
    }

    public int Width
    {
        get => BaseModel.Width;
        set => SetProperty(BaseModel.Width, value, BaseModel, (o, v) => o.Width = v);
    }

    public int Height
    {
        get => BaseModel.Height;
        set => SetProperty(BaseModel.Height, value, BaseModel, (o, v) => o.Height = v);
    }
}
