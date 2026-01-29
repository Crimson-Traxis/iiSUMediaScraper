using CommunityToolkit.Mvvm.ComponentModel;
using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.Views;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Maps view model types to their corresponding page types for navigation.
/// </summary>
public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = [];

    /// <summary>
    /// Initializes a new instance of the PageService and configures all view model to page mappings.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public PageService(ILogger<PageService> logger)
    {
        Logger = logger;
        Configure<MainViewModel, MainView>();
    }

    /// <summary>
    /// Registers a view model to page mapping.
    /// </summary>
    /// <typeparam name="VM">The view model type.</typeparam>
    /// <typeparam name="V">The page type.</typeparam>
    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            string key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            Type type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }

    /// <summary>
    /// Gets the page type associated with the specified key.
    /// </summary>
    /// <param name="key">The view model's full type name.</param>
    /// <returns>The corresponding page type.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is not found.</exception>
    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                Logger.LogError("Page not found for key: {Key}. Available keys: {AvailableKeys}", key, string.Join(", ", _pages.Keys));
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    protected ILogger Logger { get; private set; }
}
