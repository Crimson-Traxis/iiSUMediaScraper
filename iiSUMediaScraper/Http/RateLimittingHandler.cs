using System.Threading.RateLimiting;

namespace iiSUMediaScraper.Http;

/// <summary>
/// HTTP message handler that applies rate limiting to HTTP requests.
/// Ensures requests don't exceed the configured rate limit.
/// </summary>
public class RateLimitingHandler : DelegatingHandler
{
    private readonly RateLimiter _rateLimiter;

    /// <summary>
    /// Initializes a new instance of the RateLimitingHandler.
    /// </summary>
    /// <param name="rateLimiter">The rate limiter to apply to requests.</param>
    public RateLimitingHandler(RateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    /// <summary>
    /// Sends an HTTP request after acquiring a rate limit lease.
    /// Waits for rate limit availability before proceeding with the request.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to acquire rate limit lease.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using RateLimitLease lease = await _rateLimiter.AcquireAsync(1, cancellationToken);

        if (!lease.IsAcquired)
        {
            throw new InvalidOperationException("Could not acquire rate limit lease");
        }

        return await base.SendAsync(request, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _rateLimiter.Dispose();
        }
        base.Dispose(disposing);
    }
}
