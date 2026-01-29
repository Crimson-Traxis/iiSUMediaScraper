using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace iiSUMediaScraper.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the UpscalerService with dependency injection
    /// </summary>
    public static IServiceCollection AddUpscalerService(
        this IServiceCollection services,
        Action<UpscalerServiceSettings> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IUpscalerService, UpscalerService>();
        return services;
    }
}
