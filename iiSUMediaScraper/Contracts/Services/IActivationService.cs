namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service for handling application activation and startup.
/// </summary>
public interface IActivationService
{
    /// <summary>
    /// Activates the application by initializing services and handling activation arguments.
    /// </summary>
    /// <param name="activationArgs">The activation arguments from the system.</param>
    Task ActivateAsync(object activationArgs);
}
