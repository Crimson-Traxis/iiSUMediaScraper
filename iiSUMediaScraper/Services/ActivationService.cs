using iiSUMediaScraper.Activation;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper;
using iiSUMediaScraper.Views;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Handles application activation, initialization, and startup.
/// Coordinates activation handlers and sets up the main window.
/// </summary>
public class ActivationService : IActivationService
{
    private readonly UIElement? _shell = null;

    /// <summary>
    /// Initializes a new instance of the ActivationService.
    /// </summary>
    /// <param name="defaultHandler">The default activation handler for launch events.</param>
    /// <param name="activationHandlers">Collection of specialized activation handlers.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers, ILogger<ActivationService> logger)
    {
        DefaultHandler = defaultHandler;
        ActivationHandlers = activationHandlers;
        Logger = logger;
    }

    /// <summary>
    /// Delegates activation to the appropriate handler based on the activation arguments.
    /// </summary>
    private async Task HandleActivationAsync(object activationArgs)
    {
        try
        {
            var activationHandler = ActivationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null)
            {
                await activationHandler.HandleAsync(activationArgs);
            }

            if (DefaultHandler.CanHandle(activationArgs))
            {
                await DefaultHandler.HandleAsync(activationArgs);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to handle activation");
            throw;
        }
    }

    /// <summary>
    /// Performs initialization tasks before activation.
    /// </summary>
    private async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Performs startup tasks after activation.
    /// </summary>
    private async Task StartupAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Activates the application by initializing services, setting up the window, and handling activation arguments.
    /// </summary>
    /// <param name="activationArgs">The activation arguments from the system.</param>
    public async Task ActivateAsync(object activationArgs)
    {
        try
        {
            // Execute tasks before activation.
            await InitializeAsync();

            // Set the MainWindow Content.
            if (App.MainWindow.Content == null)
            {
                var root = new RootView();

                App.MainWindow.Content = _shell ?? root;

                App.MainWindow.SetTitleBar(root.Grabber);
            }

            // Handle activation via ActivationHandlers.
            await HandleActivationAsync(activationArgs);

            // Activate the MainWindow.
            App.MainWindow.Activate();

            App.MainWindow.Width = 1400;

            // Execute tasks after activation.
            await StartupAsync();
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Failed to activate application");
            throw;
        }
    }

    /// <summary>
    /// Gets the default activation handler for launch events.
    /// </summary>
    protected ActivationHandler<LaunchActivatedEventArgs> DefaultHandler { get; private set; }

    /// <summary>
    /// Gets the collection of activation handlers for processing different activation types.
    /// </summary>
    protected IEnumerable<IActivationHandler> ActivationHandlers { get; private set; }

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }
}
