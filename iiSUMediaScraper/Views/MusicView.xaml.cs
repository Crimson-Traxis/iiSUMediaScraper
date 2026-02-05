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
public sealed partial class MusicView : UserControl, INotifyPropertyChanged
{
    private MusicViewModel? _viewModel;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the MusicView.
    /// </summary>
    public MusicView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the grid tap event to toggle music selection.
    /// </summary>
    private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;

        ViewModel?.ToggleSelect();
    }

    /// <summary>
    /// Handles button tap and prevents propagation to parent elements.
    /// </summary>
    private void Button_Tapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Raises the PropertyChanged event for data binding updates.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets or sets the view model for this music control.
    /// </summary>
    public MusicViewModel? ViewModel
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