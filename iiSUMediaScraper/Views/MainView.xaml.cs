using iiSUMediaScraper.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace iiSUMediaScraper.Views;

/// <summary>
/// The main page view that displays the primary application interface.
/// Hosts the platform and game management features.
/// </summary>
public sealed partial class MainView : Page
{
    private readonly MainViewModel? _configuration;

    /// <summary>
    /// Initializes a new instance of the MainPageView.
    /// Retrieves the MainViewModel from dependency injection.
    /// </summary>
    public MainView()
    {
        ViewModel = App.GetService<MainViewModel>();

        InitializeComponent();
    }

    /// <summary>
    /// Gets the view model for this page.
    /// </summary>
    public MainViewModel ViewModel
    {
        get;
    }
}
