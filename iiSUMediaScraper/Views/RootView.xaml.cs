using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iiSUMediaScraper.Views;

/// <summary>
/// The root page that hosts the application navigation frame and title bar grabber.
/// Provides access to the navigation frame and window drag area.
/// </summary>
public sealed partial class RootView : Page
{
    /// <summary>
    /// Initializes a new instance of the RootView.
    /// </summary>
    public RootView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the window grabber grid used for dragging the window.
    /// </summary>
    public Grid Grabber => WindowGrabber;

    /// <summary>
    /// Gets the navigation frame for hosting page content.
    /// </summary>
    public Frame Frame => RootFrame;
}
