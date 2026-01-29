using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.ViewModels;

using Microsoft.UI.Xaml;

namespace iiSUMediaScraper.Activation;

/// <summary>
/// Default activation handler that navigates to the main page when no other handler processes the activation.
/// </summary>
public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Initializes a new instance of the DefaultActivationHandler.
    /// </summary>
    /// <param name="navigationService">Service for navigation.</param>
    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    /// <summary>
    /// Determines whether this handler can process the activation.
    /// Only handles activation if no content has been loaded yet.
    /// </summary>
    /// <param name="args">Launch activation arguments.</param>
    /// <returns>True if the frame has no content, indicating this is the initial launch.</returns>
    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    /// <summary>
    /// Handles activation by navigating to the main page.
    /// </summary>
    /// <param name="args">Launch activation arguments.</param>
    protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(MainViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
