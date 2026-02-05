
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace iiSUMediaScraper.Views;

/// <summary>
/// The main page view that displays the primary application interface.
/// Hosts the platform and game management features.
/// </summary>
public sealed partial class MainView : Page
{
    private readonly MainViewModel? _configuration;

    private Rect _lastKnownCroppedRegion;

    /// <summary>
    /// Initializes a new instance of the MainPageView.
    /// Retrieves the MainViewModel from dependency injection.
    /// </summary>
    public MainView()
    {
        ViewModel = App.GetService<MainViewModel>();

        ViewModel.EditRequested += ViewModel_EditRequested;

        InitializeComponent();

        ImageCropper.ManipulationCompleted += ImageCropperRight_ManipulationCompleted;
    }

    /// <summary>
    /// Handles edit requests from the view model.
    /// Loads the image into both croppers and sets the initial crop region.
    /// </summary>
    private async void ViewModel_EditRequested(object? sender, ImageViewModel e)
    {
        if (ViewModel != null && 
            ViewModel.SelectedPlatform != null && 
            ViewModel.SelectedPlatform.EditImage != null &&
            !string.IsNullOrWhiteSpace(ViewModel.SelectedPlatform.EditImage.LocalPath))
        {
            WriteableBitmap editableBitmap = await FileToWriteableBitmapAsync(ViewModel.SelectedPlatform.EditImage.LocalPath);

            ImageCropper.Source = editableBitmap;

            if (e.Crop == null)
            {
                _lastKnownCroppedRegion = new Rect(0, 0, e.Width, e.Height);
            }
            else
            {
                _lastKnownCroppedRegion = new Rect(e.Crop.Left, e.Crop.Top, e.Crop.Width, e.Crop.Height);
            }

            var aspectRatio = new AspectRatio((int)_lastKnownCroppedRegion.Width, (int)_lastKnownCroppedRegion.Height).Value;

            ImageCropper.AspectRatio = aspectRatio;

            await Task.Delay(50); // Wait for image to render

            bool isBottomValid = ImageCropper.TrySetCroppedRegion(_lastKnownCroppedRegion);
        }
    }

    /// <summary>
    /// Handles cropper manipulation completion.
    /// </summary>
    private void ImageCropperRight_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        ImageCropper.UpdateLayout();

        _lastKnownCroppedRegion = ImageCropper.CroppedRegion;
    }

    /// <summary>
    /// Handles save button click from the cropper view.
    /// Saves the current crop region to the image and updates demo mode media.
    /// </summary>
    private async void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && ViewModel.SelectedPlatform != null && ViewModel.SelectedPlatform.EditImage != null)
        {
            CropViewModel? crop = ViewModel.SelectedPlatform.EditImage.Crop;

            crop ??= new CropViewModel(new Crop());

            crop.Top = (int)Math.Round(_lastKnownCroppedRegion.Top);
            crop.Left = (int)Math.Round(_lastKnownCroppedRegion.Left);
            crop.Width = (int)Math.Round(_lastKnownCroppedRegion.Width);
            crop.Height = (int)Math.Round(_lastKnownCroppedRegion.Height);

            ViewModel.SelectedPlatform.EditImage.Crop = crop;
        }

        if (ViewModel != null && ViewModel.SelectedPlatform != null)
        {
            await ViewModel.SelectedPlatform.Save();
        }

        switch (ViewModel?.SelectedPlatform?.EditImage?.MediaType)
        {
            case MediaType.Icon:
                ViewModel?.SelectedPlatform?.EditImageGame?.UpdateDemoModeIcon();
                break;
            case MediaType.Logo:
            case MediaType.Title:
                ViewModel?.SelectedPlatform?.EditImageGame?.UpdateDemoModeTitle();
                break;
            case MediaType.Hero:
                ViewModel?.SelectedPlatform?.EditImageGame?.UpdateDemoModeHeros();
                break;
            case MediaType.Slide:
                ViewModel?.SelectedPlatform?.EditImageGame?.UpdateDemoModeSlides();
                break;

        }

        ViewModel?.SelectedPlatform?.StopEdit();
    }

    /// <summary>
    /// Loads an image from a file path to a WriteableBitmap for use in the image cropper.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <returns>A WriteableBitmap containing the image data.</returns>
    private static async Task<WriteableBitmap> FileToWriteableBitmapAsync(string filePath)
    {
        var file = await StorageFile.GetFileFromPathAsync(filePath);
        using var stream = await file.OpenReadAsync();

        var decoder = await BitmapDecoder.CreateAsync(stream);

        var writeableBitmap = new WriteableBitmap(
            (int)decoder.PixelWidth,
            (int)decoder.PixelHeight);

        stream.Seek(0);
        await writeableBitmap.SetSourceAsync(stream);

        return writeableBitmap;
    }

    /// <summary>
    /// Gets the view model for this page.
    /// </summary>
    public MainViewModel ViewModel
    {
        get;
    }
}
