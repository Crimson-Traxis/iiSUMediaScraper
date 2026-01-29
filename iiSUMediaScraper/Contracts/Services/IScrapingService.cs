using iiSUMediaScraper.Models;

namespace iiSUMediaScraper.Contracts.Services;

public interface IScrapingService
{
    Task<MediaContext> GetMedia(string platform, string game);

    Task DownloadMissingMedia(MediaContext mediaContext);
}
