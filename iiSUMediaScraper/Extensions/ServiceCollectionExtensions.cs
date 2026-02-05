using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Http;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace iiSUMediaScraper.Extensions;

public static class ServiceCollectionExtensions
{
    private static TokenBucketRateLimiter CreateRateLimiter(int tokenLimit, double replenishmentSeconds)
    {
        return new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = tokenLimit,
            ReplenishmentPeriod = TimeSpan.FromSeconds(replenishmentSeconds),
            TokensPerPeriod = tokenLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = int.MaxValue
        });
    }

    private static IServiceCollection AddApiClient(
        this IServiceCollection services,
        string name,
        RateLimiter rateLimiter,
        int? maxConnections = null)
    {
        var builder = services.AddHttpClient(name, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        if (maxConnections.HasValue)
        {
            builder.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                MaxConnectionsPerServer = maxConnections.Value
            });
        }

        builder.AddHttpMessageHandler(() => new RateLimitingHandler(rateLimiter));

        return services;
    }

    private static IServiceCollection AddDownloadClient(
        this IServiceCollection services,
        string name,
        RateLimiter rateLimiter)
    {
        services.AddHttpClient(name, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            MaxConnectionsPerServer = 4
        }).AddHttpMessageHandler(() => new RateLimitingHandler(rateLimiter));

        return services;
    }

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

    /// <summary>
    /// Registers all HTTP clients for scraping and downloading with rate limiting.
    /// Configures separate rate limiters per source to prevent starvation.
    /// </summary>
    public static IServiceCollection AddScrapingHttpClients(this IServiceCollection services)
    {
        // API rate limiters for scraping
        var igdbRateLimiter = CreateRateLimiter(tokenLimit: 4, replenishmentSeconds: 1.5);
        var steamGridDbRateLimiter = CreateRateLimiter(tokenLimit: 4, replenishmentSeconds: 1);
        var ignRateLimiter = CreateRateLimiter(tokenLimit: 4, replenishmentSeconds: 1);

        // Download rate limiters - separate per source to prevent one source from starving others
        var downloadIgdbRateLimiter = CreateRateLimiter(tokenLimit: 4, replenishmentSeconds: 1);
        var downloadSteamGridDbRateLimiter = CreateRateLimiter(tokenLimit: 4, replenishmentSeconds: 1);
        var downloadIgnRateLimiter = CreateRateLimiter(tokenLimit: 4, replenishmentSeconds: 1);
        var downloadYoutubeRateLimiter = CreateRateLimiter(tokenLimit: 4, replenishmentSeconds: 1);
        var downloadDefaultRateLimiter = CreateRateLimiter(tokenLimit: 4, replenishmentSeconds: 1);

        // API clients for scraping
        services.AddApiClient($"{SourceFlag.Igdb}", igdbRateLimiter);
        services.AddApiClient($"{SourceFlag.Ign}", ignRateLimiter, maxConnections: 4);
        services.AddApiClient($"{SourceFlag.SteamGridDb}", steamGridDbRateLimiter, maxConnections: 4);

        // Download clients - separate per source
        services.AddDownloadClient($"Download_{SourceFlag.Igdb}", downloadIgdbRateLimiter);
        services.AddDownloadClient($"Download_{SourceFlag.SteamGridDb}", downloadSteamGridDbRateLimiter);
        services.AddDownloadClient($"Download_{SourceFlag.Ign}", downloadIgnRateLimiter);
        services.AddDownloadClient($"Download_{SourceFlag.Youtube}", downloadYoutubeRateLimiter);
        services.AddDownloadClient("Download", downloadDefaultRateLimiter);

        return services;
    }
}
