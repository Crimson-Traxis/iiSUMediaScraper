using iiSUMediaScraper.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.IO;

namespace iiSUMediaScraper.Services.FileHandlers;

/// <summary>
/// File handler for the local Windows filesystem.
/// Acts as the default fallback handler when no protocol-specific handler matches.
/// </summary>
public class WindowsFileHandler : IFileHandler
{
    private readonly ILogger<WindowsFileHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsFileHandler"/> class.
    /// </summary>
    public WindowsFileHandler(ILogger<WindowsFileHandler> logger)
    {
        _logger = logger;
    }

    #region IFileHandler

    /// <summary>
    /// Returns true for any path — this is the fallback handler.
    /// Must be registered last in the handler collection.
    /// </summary>
    public bool CanHandle(string? path) => true;

    /// <summary>
    /// Reads a file as raw bytes from the local filesystem.
    /// </summary>
    public async Task<byte[]> ReadBytes(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                return await File.ReadAllBytesAsync(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read bytes from file: {FilePath}", filePath);
        }

        return [];
    }

    /// <summary>
    /// Saves raw bytes to a file on the local filesystem.
    /// Creates parent directories if needed.
    /// </summary>
    public async Task SaveBytes(string filePath, byte[] bytes)
    {
        try
        {
            var dir = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllBytesAsync(filePath, bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save bytes to file: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Deletes a file from the local filesystem.
    /// </summary>
    public Task DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes all files matching the search pattern from a local folder.
    /// </summary>
    public Task DeleteFiles(string folderPath, string? searchPattern = null)
    {
        return Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    return;
                }

                foreach (string file in Directory.EnumerateFiles(folderPath, searchPattern ?? "*"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete file: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enumerate files from: {FolderPath} with pattern: {SearchPattern}", folderPath, searchPattern);
            }
        });
    }

    /// <summary>
    /// Gets all subdirectories matching the search pattern from a local folder.
    /// </summary>
    public Task<IEnumerable<string>> GetSubFolders(string folderPath, string? searchPattern = null)
    {
        try
        {
            IEnumerable<string> result = searchPattern == null
                ? Directory.EnumerateDirectories(folderPath)
                : Directory.EnumerateDirectories(folderPath, searchPattern);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get subdirectories from: {FolderPath}", folderPath);
            return Task.FromResult<IEnumerable<string>>([]);
        }
    }

    /// <summary>
    /// Gets all files matching the search pattern from a local folder.
    /// </summary>
    public Task<IEnumerable<string>> GetFiles(string folderPath, string? searchPattern = null)
    {
        try
        {
            IEnumerable<string> result = searchPattern == null
                ? Directory.EnumerateFiles(folderPath)
                : Directory.EnumerateFiles(folderPath, searchPattern);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get files from: {FolderPath}", folderPath);
            return Task.FromResult<IEnumerable<string>>([]);
        }
    }

    /// <summary>
    /// Creates a directory on the local filesystem.
    /// </summary>
    public Task CreateDirectory(string folderPath)
    {
        try
        {
            Directory.CreateDirectory(folderPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create directory: {FolderPath}", folderPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if a file exists on the local filesystem.
    /// </summary>
    public Task<bool> FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(File.Exists(filePath));
    }

    /// <summary>
    /// Gets the folder name from a local filesystem path.
    /// </summary>
    public string GetFolderName(string folderPath)
    {
        try
        {
            return new DirectoryInfo(folderPath).Name;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get folder name from path: {FolderPath}", folderPath);

            return string.Empty;
        }
    }

    /// <summary>
    /// Combines path segments using the local filesystem separator.
    /// </summary>
    public string CombinePath(string basePath, params string[] segments)
    {
        var all = new string[segments.Length + 1];
        all[0] = basePath;
        segments.CopyTo(all, 1);

        return Path.Combine(all.Where(s => !string.IsNullOrEmpty(s)).ToArray());
    }

    /// <summary>
    /// Copies a file on the local filesystem.
    /// </summary>
    public Task CopyFile(string source, string destination)
    {
        return Task.Run(() =>
        {
            try
            {
                if (!File.Exists(source))
                {
                    _logger.LogWarning("Cannot copy file: source does not exist at {Source}", source);
                    return;
                }

                var destinationDir = Path.GetDirectoryName(destination);

                if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                File.Copy(source, destination, overwrite: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy file from {Source} to {Destination}", source, destination);
            }
        });
    }

    /// <summary>
    /// Moves a file on the local filesystem.
    /// </summary>
    public Task MoveFile(string source, string destination)
    {
        return Task.Run(() =>
        {
            try
            {
                if (!File.Exists(source))
                {
                    _logger.LogWarning("Cannot move file: source does not exist at {Source}", source);
                    return;
                }

                var destinationDir = Path.GetDirectoryName(destination);

                if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                File.Move(source, destination, overwrite: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move file from {Source} to {Destination}", source, destination);
            }
        });
    }

    /// <summary>
    /// Returns the path unchanged — local filesystem does not rewrite paths.
    /// </summary>
    public Task<string?> CheckPath(string? path) => Task.FromResult(path);

    /// <summary>
    /// No-op for local filesystem — no connections to clean up.
    /// </summary>
    public Task Cleanup() => Task.CompletedTask;

    #endregion
}
