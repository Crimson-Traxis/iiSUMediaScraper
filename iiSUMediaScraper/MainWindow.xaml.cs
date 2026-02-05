using iiSUMediaScraper.Helpers;
using iiSUMediaScraper.Services;
using System.IO;
using Windows.UI.ViewManagement;

namespace iiSUMediaScraper;

/// <summary>
/// The main application window.
/// Handles window initialization, theme changes, and title bar customization.
/// </summary>
public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private readonly UISettings settings;

    /// <summary>
    /// Initializes a new instance of the MainWindow.
    /// Sets window icon, title, size, and configures system theme change handling.
    /// Extends content into the title bar for a modern appearance.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1900, 900));

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        UIThreadService.DispatcherQueue = dispatcherQueue;

        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        ExtendsContentIntoTitleBar = true;
    }

    /// <summary>
    /// Handles Windows system theme changes.
    /// Updates caption button colors to match the new theme.
    /// This event comes from off-thread, so it's dispatched to the UI thread.
    /// </summary>
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}
