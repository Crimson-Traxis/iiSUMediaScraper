using iiSUMediaScraper.Activation;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Extensions;
using iiSUMediaScraper.Services;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.UI.Xaml;
using System.IO;

namespace iiSUMediaScraper;

/// <summary>
/// The main application class for iiSU Media Scraper.
/// Configures dependency injection, HTTP clients with rate limiting,
/// and registers all services, view models, and views.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Gets the .NET Generic Host that provides dependency injection, configuration, and other services.
    /// </summary>
    public IHost Host
    {
        get;
    }

    /// <summary>
    /// Retrieves a service from the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The service type to retrieve.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the service is not registered.</exception>
    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    /// <summary>
    /// Gets the main application window.
    /// </summary>
    public static WindowEx MainWindow { get; } = new MainWindow();

    /// <summary>
    /// Gets or sets the application title bar UI element.
    /// </summary>
    public static UIElement? AppTitlebar { get; set; }

    private ILogger<App>? _logger;

    /// <summary>
    /// Initializes a new instance of the App class.
    /// Configures the dependency injection container with all services, HTTP clients, and views.
    /// Sets up rate limiting for IGDB API calls and connection pooling for other services.
    /// </summary>
    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
#if DEBUG
            config.AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true);
#endif
        }).
        ConfigureServices((context, services) =>
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                builder.AddDebug();
                builder.AddEventLog();
            });

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Services
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IScrapingService, ScrapingService>();
            services.AddSingleton<IMediaFormatterService, MediaFormatterService>();
            services.AddSingleton<IUIThreadService, UIThreadService>();
            services.AddSingleton<IDownloader, Downloader>();

            services.AddUpscalerService(options =>
            {
                options.ServerDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpscaleServer");
            });

            // HTTP clients for scraping and downloading
            services.AddScrapingHttpClients();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainView>();

            // Configuration
        }).
        Build();

        UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        _logger = Host.Services.GetService<ILogger<App>>();
    }

    /// <summary>
    /// Handles unhandled exceptions on the UI thread.
    /// Logs the exception details for diagnostics.
    /// </summary>
    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        _logger?.LogCritical(e.Exception, "Unhandled UI exception occurred: {Message}", e.Message);
        e.Handled = true;
    }

    /// <summary>
    /// Handles unhandled exceptions on non-UI threads.
    /// Logs the exception details for diagnostics.
    /// </summary>
    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _logger?.LogCritical(exception, "Unhandled domain exception occurred. IsTerminating: {IsTerminating}", e.IsTerminating);
        }
        else
        {
            _logger?.LogCritical("Unhandled domain exception occurred with non-Exception object: {ExceptionObject}. IsTerminating: {IsTerminating}", e.ExceptionObject, e.IsTerminating);
        }
    }

    /// <summary>
    /// Handles unobserved task exceptions.
    /// Logs the exception details for diagnostics.
    /// </summary>
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger?.LogError(e.Exception, "Unobserved task exception occurred");
        e.SetObserved();
    }

    /// <summary>
    /// Handles application launch.
    /// Activates the application using the IActivationService.
    /// </summary>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
