namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service for dispatching actions to the UI thread from background threads.
/// </summary>
public interface IUIThreadService
{
    /// <summary>
    /// Dispatches an action to run on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread.</param>
    /// <returns>A completed task.</returns>
    Task DispachToUIThread(Action action);
}
