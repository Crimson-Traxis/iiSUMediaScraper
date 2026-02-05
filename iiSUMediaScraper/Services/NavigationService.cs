using iiSUMediaScraper;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Contracts.ViewModels;
using iiSUMediaScraper.Helpers;
using iiSUMediaScraper.Views;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics.CodeAnalysis;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Manages page navigation within the WinUI application.
/// Handles forward/backward navigation and notifies view models of navigation events.
/// For more information on navigation between pages see
/// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
/// </summary>
public class NavigationService : INavigationService
{
    private object? _lastParameterUsed;
    private Frame? _frame;

    /// <summary>
    /// Raised when navigation to a page completes.
    /// </summary>
    public event NavigatedEventHandler? Navigated;

    /// <summary>
    /// Initializes a new instance of the NavigationService.
    /// </summary>
    /// <param name="pageService">Service for resolving page types by key.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public NavigationService(IPageService pageService, ILogger<NavigationService> logger)
    {
        PageService = pageService;
        Logger = logger;
    }

    /// <summary>
    /// Registers event handlers for the navigation frame.
    /// </summary>
    private void RegisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated += OnNavigated;
        }
    }

    /// <summary>
    /// Unregisters event handlers from the navigation frame.
    /// </summary>
    private void UnregisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated -= OnNavigated;
        }
    }

    /// <summary>
    /// Handles the Navigated event from the frame.
    /// Clears back stack if requested and notifies view models of navigation.
    /// </summary>
    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        try
        {
            if (sender is Frame frame)
            {
                bool clearNavigation = frame.Tag is bool clear && clear;
                if (clearNavigation)
                {
                    frame.BackStack.Clear();
                }

                if (frame.GetPageViewModel() is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(e.Parameter);
                }

                Navigated?.Invoke(sender, e);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to handle navigation event for {SourcePageType}", e.SourcePageType?.Name);
        }
    }

    /// <summary>
    /// Navigates to the previous page in the back stack.
    /// Notifies the previous view model of the navigation.
    /// </summary>
    /// <returns>True if navigation succeeded, false otherwise.</returns>
    public bool GoBack()
    {
        try
        {
            if (CanGoBack)
            {
                object? vmBeforeNavigation = _frame.GetPageViewModel();
                _frame.GoBack();
                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to navigate back");
        }

        return false;
    }

    /// <summary>
    /// Navigates to a page specified by key.
    /// </summary>
    /// <param name="pageKey">The unique key identifying the page.</param>
    /// <param name="parameter">Optional navigation parameter to pass to the page.</param>
    /// <param name="clearNavigation">Whether to clear the navigation back stack.</param>
    /// <returns>True if navigation succeeded, false otherwise.</returns>
    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        try
        {
            var pageType = PageService.GetPageType(pageKey);

            if (_frame != null && (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed))))
            {
                _frame.Tag = clearNavigation;
                object? vmBeforeNavigation = _frame.GetPageViewModel();
                bool navigated = _frame.Navigate(pageType, parameter);
                if (navigated)
                {
                    _lastParameterUsed = parameter;
                    if (vmBeforeNavigation is INavigationAware navigationAware)
                    {
                        navigationAware.OnNavigatedFrom();
                    }
                }

                return navigated;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to navigate to page {PageKey}", pageKey);
        }

        return false;
    }

    /// <summary>
    /// Gets the page service for resolving page types.
    /// </summary>
    protected IPageService PageService { get; private set; }

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }

    /// <summary>
    /// Gets or sets the navigation frame.
    /// </summary>
    public Frame? Frame
    {
        get
        {
            try
            {
                if (_frame == null)
                {
                    _frame = (App.MainWindow.Content as RootView)?.Frame;
                    RegisterFrameEvents();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to get navigation frame from main window");
            }

            return _frame;
        }

        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the frame can navigate back.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Frame), nameof(_frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;
}
