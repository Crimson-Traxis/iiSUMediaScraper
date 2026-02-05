using iiSUMediaScraper.Models;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the PlatformView.
    /// Wires up event handlers for image cropper synchronization and responsive layout.
    /// </summary>
    public PlatformView()
    {
        InitializeComponent();

        ResizeControls(new Size(ActualWidth, ActualHeight));

        GridContent.SizeChanged += GridContent_SizeChanged;

        Loaded += PlatformView_Loaded;
    }

    /// <summary>
    /// Handles when the control is loaded.
    /// Switches between side-by-side and stacked layouts at 1200px width threshold.
    /// </summary>
    private void PlatformView_Loaded(object sender, RoutedEventArgs e)
    {
        ResizeControls(new Size(ActualWidth, ActualHeight));
    }

    /// <summary>
    /// Handles grid content size changes to implement responsive layout.
    /// Switches between side-by-side and stacked layouts at 1200px width threshold.
    /// </summary>
    private void GridContent_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ResizeControls(e.NewSize);
    }

    /// <summary>
    /// Handles when showing/hiding previewer to implement responsive layout.
    /// Switches between side-by-side and stacked layouts at 1200px width threshold if previewer is shown.
    /// </summary>
    private void Configuration_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(ConfigurationViewModel.IsPreviewerEnabled))
        {
            ResizeControls(new Size(ActualWidth, ActualHeight));
        }
    }

    /// <summary>
    /// Handles grid content size changes to implement responsive layout.
    /// Switches between side-by-side and stacked layouts at 1200px width threshold.
    /// </summary>
    private void ResizeControls(Size size)
    {
        if (ViewModel?.Configuration?.IsPreviewerEnabled ?? false)
        {
            if (size.Width > 1200)
            {
                ImageViewSide.Width = new GridLength(1, GridUnitType.Star);

                ImageViewSide.MaxWidth = 800;

                CardViewSide.Width = new GridLength(1, GridUnitType.Star);

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
        else
        {
            ImageViewSide.Width = new GridLength(0, GridUnitType.Pixel);

            ImageViewSide.MaxWidth = 0;

            CardViewSide.Width = new GridLength(1, GridUnitType.Star);

            CardViewTop.Height = new GridLength(1, GridUnitType.Star);

            BotomViewSide.Height = new GridLength(0, GridUnitType.Pixel);
        }
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
                if (_viewModel != null && _viewModel.Configuration != null)
                {
                    _viewModel.Configuration.PropertyChanged -= Configuration_PropertyChanged;
                }

                _viewModel = value;

                OnPropertyChanged(nameof(ViewModel));

                if(_viewModel != null && _viewModel.Configuration != null)
                {
                    _viewModel.Configuration.PropertyChanged += Configuration_PropertyChanged;
                }

                ResizeControls(new Size(ActualWidth, ActualHeight));
            }
        }
    }
}