using iiSUMediaScraper.Models;

namespace iiSUMediaScraper.Contracts.Services;

public interface IImageFormatterService
{
    Task UpdateImageSize(Image image);

    Task<Image?> FormatIcon(Image image, string platform);

    Task<Image?> FormatTitle(Image image);

    Task<Image?> FormatLogo(Image image);

    Task<Image?> FormatHero(Image image);

    Task<Image?> FormatSlide(Image image);

    Task<Crop?> SmartCropIcon(Image image);

    Task<Crop?> SmartCropTitle(Image image);

    Task<Crop?> SmartCropLogo(Image image);

    Task<Crop?> SmartCropHero(Image image);

    Task<Crop?> SmartCropSlide(Image image);

    Task<Crop?> CalculateNewCrop(int oldWidth, int oldHeight, Crop oldCrop, int newWidth, int newHeight);
}
