using iiSUMediaScraper.Controls;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iiSUMediaScraper.Views;

/// <summary>
/// A user control for displaying a platform menu item.
/// Adapts its layout based on available width, switching between large and compact modes.
/// </summary>
public sealed partial class PlatformMenuItemView : UserControl, INotifyPropertyChanged
{
    private PlatformViewModel? _viewModel;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the PlatformMenuItemView.
    /// Wires up the size changed handler for responsive layout.
    /// </summary>
    public PlatformMenuItemView()
    {
        InitializeComponent();

        SizeChanged += PlatformMenuItemView_SizeChanged;
    }

    /// <summary>
    /// Handles size changes to switch between large and compact layouts.
    /// </summary>
    private void PlatformMenuItemView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        IsLarge = e.NewSize.Width > 100;
    }

    /// <summary>
    /// Raises the PropertyChanged event for data binding updates.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets or sets the view model for this platform menu item.
    /// </summary>
    public PlatformViewModel? ViewModel
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

    /// <summary>
    /// Gets or sets a value indicating whether the menu item should use a large layout.
    /// Automatically switches based on control width.
    /// </summary>
    public bool IsLarge
    {
        get => (bool)GetValue(IsLargeProperty);
        set => SetValue(IsLargeProperty, value);
    }

    /// <summary>
    /// Identifies the IsLarge dependency property.
    /// </summary>
    public static readonly DependencyProperty IsLargeProperty =
        DependencyProperty.Register(
            nameof(IsLarge),
            typeof(bool),
            typeof(PlatformMenuItemView),
            new PropertyMetadata(true));
}
