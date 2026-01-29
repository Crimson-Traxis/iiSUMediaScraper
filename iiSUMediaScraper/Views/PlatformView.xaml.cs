using iiSUMediaScraper.Models;
using iiSUMediaScraper.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iiSUMediaScraper.Views;

/// <summary>
/// A user control that displays all games for a specific platform.
/// Features dual image croppers (bottom and right) that stay synchronized,
/// and responsive layout that adapts to window size.
/// </summary>
public sealed partial class PlatformView : UserControl, INotifyPropertyChanged
{
    private PlatformViewModel? _viewModel;

    private Rect _lastKnownCroppedRegion;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the PlatformView.
    /// Wires up event handlers for image cropper synchronization and responsive layout.
    /// </summary>
    public PlatformView()
    {
        InitializeComponent();

        GridContent.SizeChanged += GridContent_SizeChanged;

        ImageCropperBottom.ManipulationCompleted += ImageCropperBottom_ManipulationCompleted;

        ImageCropperBottom.Loaded += ImageCropperBottom_Loaded;

        ImageCropperRight.ManipulationCompleted += ImageCropperRight_ManipulationCompleted;

        ImageCropperRight.Loaded += ImageCropperRight_Loaded;
    }

    /// <summary>
    /// Synchronizes the right cropper with the bottom cropper on load.
    /// </summary>
    private void ImageCropperRight_Loaded(object sender, RoutedEventArgs e)
    {
        ImageCropperRight.TrySetCroppedRegion(ImageCropperBottom.CroppedRegion);
    }

    /// <summary>
    /// Synchronizes the bottom cropper with the right cropper on load.
    /// </summary>
    private void ImageCropperBottom_Loaded(object sender, RoutedEventArgs e)
    {
        ImageCropperBottom.TrySetCroppedRegion(ImageCropperRight.CroppedRegion);
    }

    /// <summary>
    /// Handles right cropper manipulation completion.
    /// Updates the last known crop region and synchronizes with the bottom cropper.
    /// </summary>
    private void ImageCropperRight_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        ImageCropperRight.UpdateLayout();

        _lastKnownCroppedRegion = ImageCropperRight.CroppedRegion;

        ImageCropperBottom.TrySetCroppedRegion(ImageCropperRight.CroppedRegion);
    }

    /// <summary>
    /// Handles bottom cropper manipulation completion.
    /// Updates the last known crop region and synchronizes with the right cropper.
    /// </summary>
    private void ImageCropperBottom_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        ImageCropperBottom.UpdateLayout();

        _lastKnownCroppedRegion = ImageCropperBottom.CroppedRegion;

        ImageCropperRight.TrySetCroppedRegion(ImageCropperBottom.CroppedRegion);
    }

    /// <summary>
    /// Handles grid content size changes to implement responsive layout.
    /// Switches between side-by-side and stacked layouts at 1200px width threshold.
    /// </summary>
    private void GridContent_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > 1200)
        {
            ImageViewSide.Width = new GridLength(1, GridUnitType.Star);

            CardViewSide.Width = new GridLength(700, GridUnitType.Pixel);

            CardViewTop.Height = new GridLength(1, GridUnitType.Star);

            BotomViewSide.Height = new GridLength(0, GridUnitType.Pixel);
        }
        else
        {
            ImageViewSide.Width = new GridLength(0, GridUnitType.Pixel);

            CardViewSide.Width = new GridLength(1, GridUnitType.Star);

            CardViewTop.Height = new GridLength(1, GridUnitType.Star);

            BotomViewSide.Height = new GridLength(400, GridUnitType.Pixel);
        }
    }

    /// <summary>
    /// Handles edit requests from the view model.
    /// Loads the image into both croppers and sets the initial crop region.
    /// </summary>
    private async void ViewModel_EditRequested(object? sender, ImageViewModel e)
    {
        if (ViewModel != null && ViewModel.EditImage != null)
        {
            WriteableBitmap editableBitmap = await ByteArrayToWriteableBitmapAsync(ViewModel.EditImage.Bytes);

            ImageCropperBottom.Source = editableBitmap;

            ImageCropperRight.Source = editableBitmap;

            if (e.Crop == null)
            {
                _lastKnownCroppedRegion = new Rect(0, 0, e.Width, e.Height);
            }
            else
            {
                _lastKnownCroppedRegion = new Rect(e.Crop.Left, e.Crop.Top, e.Crop.Width, e.Crop.Height);
            }

            var aspectRatio = new AspectRatio((int)_lastKnownCroppedRegion.Width, (int)_lastKnownCroppedRegion.Height).Value;

            ImageCropperBottom.AspectRatio = aspectRatio;

            ImageCropperRight.AspectRatio = aspectRatio;

            await Task.Delay(50); // Wait for image to render

            bool isBottomValid = ImageCropperBottom.TrySetCroppedRegion(_lastKnownCroppedRegion);

            bool isRightValid = ImageCropperRight.TrySetCroppedRegion(_lastKnownCroppedRegion);
        }
    }

    /// <summary>
    /// Handles save button click from the right cropper view.
    /// Saves the current crop region to the image and updates demo mode media.
    /// </summary>
    private async void ButtonSaveRight_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && ViewModel.EditImage != null)
        {
            CropViewModel? crop = ViewModel.EditImage.Crop;

            crop ??= new CropViewModel(new Crop());

            crop.Top = (int)Math.Round(_lastKnownCroppedRegion.Top);
            crop.Left = (int)Math.Round(_lastKnownCroppedRegion.Left);
            crop.Width = (int)Math.Round(_lastKnownCroppedRegion.Width);
            crop.Height = (int)Math.Round(_lastKnownCroppedRegion.Height);

            ViewModel.EditImage.Crop = crop;
        }

        if (ViewModel != null)
        {
            await ViewModel.Save();
        }

        ViewModel?.EditImageGame?.UpdateDemoModeMedia();

        ViewModel?.StopEdit();
    }

    /// <summary>
    /// Handles save button click from the bottom cropper view.
    /// Saves the current crop region to the image and updates demo mode media.
    /// </summary>
    private async void ButtonSaveBottom_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && ViewModel.EditImage != null)
        {
            CropViewModel? crop = ViewModel.EditImage.Crop;

            crop ??= new CropViewModel(new Crop());

            crop.Top = (int)Math.Round(_lastKnownCroppedRegion.Top);
            crop.Left = (int)Math.Round(_lastKnownCroppedRegion.Left);
            crop.Width = (int)Math.Round(_lastKnownCroppedRegion.Width);
            crop.Height = (int)Math.Round(_lastKnownCroppedRegion.Height);

            ViewModel.EditImage.Crop = crop;
        }

        if (ViewModel != null)
        {
            await ViewModel.Save();
        }

        ViewModel?.EditImageGame?.UpdateDemoModeMedia();

        ViewModel?.StopEdit();
    }

    /// <summary>
    /// Converts a byte array to a WriteableBitmap for use in the image cropper.
    /// </summary>
    /// <param name="bytes">The image bytes to convert.</param>
    /// <returns>A WriteableBitmap containing the image data.</returns>
    private static async Task<WriteableBitmap> ByteArrayToWriteableBitmapAsync(byte[] bytes)
    {
        using InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(bytes.AsBuffer());
        stream.Seek(0);

        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

        WriteableBitmap writeableBitmap = new WriteableBitmap(
            (int)decoder.PixelWidth,
            (int)decoder.PixelHeight);

        stream.Seek(0);
        await writeableBitmap.SetSourceAsync(stream);

        return writeableBitmap;
    }

    /// <summary>
    /// Raises the PropertyChanged event for data binding updates.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets or sets the view model for this platform control.
    /// Automatically wires up/unwires event handlers for edit requests.
    /// </summary>
    public PlatformViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != value)
            {
                if (ViewModel != null)
                {
                    ViewModel.EditRequested -= ViewModel_EditRequested;
                }

                _viewModel = value;

                if (ViewModel != null)
                {
                    ViewModel.EditRequested += ViewModel_EditRequested;
                }

                OnPropertyChanged(nameof(ViewModel));
            }
        }
    }
}