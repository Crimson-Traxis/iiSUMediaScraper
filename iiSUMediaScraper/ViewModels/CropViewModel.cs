using iiSUMediaScraper.Models;
using iiSUMediaScraper.ObservableModels;

namespace iiSUMediaScraper.ViewModels;

/// <summary>
/// Represents the view model for image crop settings.
/// </summary>
public class CropViewModel : ObservableCrop
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CropViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying crop model.</param>
    public CropViewModel(Crop baseModel) : base(baseModel)
    {
    }
}
