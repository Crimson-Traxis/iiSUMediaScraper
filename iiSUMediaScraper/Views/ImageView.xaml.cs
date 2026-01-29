using iiSUMediaScraper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iiSUMediaScraper.Views;

/// <summary>
/// A user control that displays a single image with selection and preview functionality.
/// Supports hover previews and click-to-select behavior for media management.
/// </summary>
public sealed partial class ImageView : UserControl, INotifyPropertyChanged
{
    private ImageViewModel? _viewModel;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the ImageView.
    /// </summary>
    public ImageView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the edit button tap event and prevents propagation to parent elements.
    /// </summary>
    private void ButtonEdit_Tapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Handles the grid tap event to toggle image selection.
    /// </summary>
    private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel?.ToggleSelect();
    }

    /// <summary>
    /// Handles pointer entered event to show hover state and request preview.
    /// </summary>
    private async void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.IsHovering = true;

            await ViewModel.RequestPreview();
        }
    }

    /// <summary>
    /// Handles pointer exited event to hide hover state and stop preview.
    /// </summary>
    private async void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.IsHovering = false;

            await ViewModel.RequestStopPreview();
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
    /// Calculates the scaled width to maintain aspect ratio with a target height of 140 pixels.
    /// </summary>
    /// <param name="width">Original image width.</param>
    /// <param name="height">Original image height.</param>
    /// <returns>Scaled width, or NaN if height is zero.</returns>
    public double CalculateScaledWidth(int width, int height)
    {
        const double targetHeight = 140;
        return height > 0 ? width * (targetHeight / height) : double.NaN;
    }

    /// <summary>
    /// Gets or sets the view model for this image control.
    /// </summary>
    public ImageViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != value)
            {
                _viewModel = value;

                OnPropertyChanged(nameof(ViewModel));
            }
        }
    }
}