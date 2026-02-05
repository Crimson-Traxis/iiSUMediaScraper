using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service for managing page navigation within the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Raised when navigation to a page completes.
    /// </summary>
    event NavigatedEventHandler Navigated;

    /// <summary>
    /// Gets a value indicating whether the frame can navigate back.
    /// </summary>
    bool CanGoBack
    {
        get;
    }

    /// <summary>
    /// Gets or sets the navigation frame.
    /// </summary>
    Frame? Frame
    {
        get; set;
    }

    /// <summary>
    /// Navigates to a page specified by key.
    /// </summary>
    /// <param name="pageKey">The unique key identifying the page.</param>
    /// <param name="parameter">Optional navigation parameter to pass to the page.</param>
    /// <param name="clearNavigation">Whether to clear the navigation back stack.</param>
    /// <returns>True if navigation succeeded, false otherwise.</returns>
    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    /// <summary>
    /// Navigates to the previous page in the back stack.
    /// </summary>
    /// <returns>True if navigation succeeded, false otherwise.</returns>
    bool GoBack();
}
