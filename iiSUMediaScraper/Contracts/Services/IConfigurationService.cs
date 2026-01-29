using iiSUMediaScraper.Models.Configurations;

namespace iiSUMediaScraper.Contracts.Services;

public interface IConfigurationService
{
    Task SaveConfiguration();

    Task LoadConfiguration();

    Configuration? Configuration { get; set; }
}
