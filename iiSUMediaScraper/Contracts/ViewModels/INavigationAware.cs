namespace iiSUMediaScraper.Contracts.ViewModels;

/// <summary>
/// Interface for view models that need to be notified of navigation events.
/// </summary>
public interface INavigationAware
{
    /// <summary>
    /// Called when navigating to this view model.
    /// </summary>
    /// <param name="parameter">The navigation parameter passed to the page.</param>
    void OnNavigatedTo(object parameter);

    /// <summary>
    /// Called when navigating away from this view model.
    /// </summary>
    void OnNavigatedFrom();
}
