using FluentFTP;
using iiSUMediaScraper.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Enumeration;

namespace iiSUMediaScraper.Services.FileHandlers;

/// <summary>
/// File handler for FTP/FTPS servers using FluentFTP.
/// Maintains a connection cache keyed by host:port:user.
/// All public I/O methods run on the thread pool via RunFtpOperation
/// to avoid blocking the UI thread, with a SemaphoreSlim to serialize
/// access (AsyncFtpClient is not thread-safe).
/// </summary>
public class FtpFileHandler : IFileHandler
{
    private const string FtpPrefix = "ftp://";
    private readonly ConcurrentDictionary<string, AsyncFtpClient> _ftpClients = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _ftpLock = new(1, 1);
    private readonly ILogger<FtpFileHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpFileHandler"/> class.
    /// </summary>
    public FtpFileHandler(ILogger<FtpFileHandler> logger)
    {
        _logger = logger;
    }

    #region Private FTP Helpers

    private Task<T> RunFtpOperation<T>(Func<Task<T>> operation)
    {
        return Task.Run(async () =>
        {
            await _ftpLock.WaitAsync();
            try
            {
                return await operation();
            }
            finally
            {
                _ftpLock.Release();
            }
        });
    }

    private Task RunFtpOperation(Func<Task> operation)
    {
        return Task.Run(async () =>
        {
            await _ftpLock.WaitAsync();
            try
            {
                await operation();
            }
            finally
            {
                _ftpLock.Release();
            }
        });
    }

    private (string host, int port, string? user, string? pass, string remotePath) ParseFtpUri(string path)
    {
        var uri = new Uri(path);
        string? user = null;
        string? pass = null;

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            user = Uri.UnescapeDataString(parts[0]);
            if (parts.Length > 1)
            {
                pass = Uri.UnescapeDataString(parts[1]);
            }
        }

        var port = uri.Port > 0 ? uri.Port : 21;

        return (uri.Host, port, user, pass, Uri.UnescapeDataString(uri.AbsolutePath));
    }

    private string FtpBaseUri(string host, int port, string? user, string? pass)
    {
        var userInfo = "";

        if (!string.IsNullOrEmpty(user))
        {
            userInfo = pass != null
                ? $"{Uri.EscapeDataString(user)}:{Uri.EscapeDataString(pass)}@"
                : $"{Uri.EscapeDataString(user)}@";
        }

        return $"ftp://{userInfo}{host}:{port}";
    }

    private string FtpCombinePaths(string basePath, params string[] segments)
    {
        var result = basePath.TrimEnd('/');

        foreach (var segment in segments.Where(s => !string.IsNullOrEmpty(s)))
        {
            result += "/" + segment.Trim('/');
        }

        return result;
    }

    private async Task<AsyncFtpClient> GetOrConnectFtpClient(string host, int port, string? user, string? pass)
    {
        var key = $"{host}:{port}:{user}";

        if (_ftpClients.TryGetValue(key, out var cached) && cached.IsConnected)
        {
            return cached;
        }

        var client = new AsyncFtpClient(host, user ?? "", pass ?? "", port);
        await client.Connect();
        _ftpClients[key] = client;
        _logger.LogInformation("Connected to FTP server: {Host}:{Port}", host, port);

        return client;
    }

    private async Task<(AsyncFtpClient client, string remotePath)> GetFtpClientForPath(string path)
    {
        var (host, port, user, pass, remotePath) = ParseFtpUri(path);
        var client = await GetOrConnectFtpClient(host, port, user, pass);

        return (client, remotePath);
    }

    private string ToFtpFullPath(string host, int port, string? user, string? pass, string remotePath)
    {
        return FtpBaseUri(host, port, user, pass) + remotePath;
    }

    private bool MatchesPattern(string fileName, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern) || pattern == "*")
        {
            return true;
        }

        return FileSystemName.MatchesSimpleExpression(pattern, fileName);
    }

    private void InvalidateClient(string path)
    {
        try
        {
            var (host, port, user, _, _) = ParseFtpUri(path);
            var key = $"{host}:{port}:{user}";

            if (_ftpClients.TryRemove(key, out var client))
            {
                try
                {
                    client.Dispose();
                }
                catch
                {
                }

                _logger.LogInformation("Invalidated FTP client for {Key} due to error", key);
            }
        }
        catch
        {
        }
    }

    #endregion

    #region IFileHandler

    /// <summary>
    /// Returns true if the path starts with the ftp:// prefix.
    /// </summary>
    public bool CanHandle(string? path) =>
        !string.IsNullOrEmpty(path) && path.StartsWith(FtpPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Reads a file as raw bytes from an FTP server.
    /// </summary>
    public Task<byte[]> ReadBytes(string filePath)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (client, remotePath) = await GetFtpClientForPath(filePath);

                if (!await client.FileExists(remotePath))
                {
                    return Array.Empty<byte>();
                }

                var bytes = await client.DownloadBytes(remotePath, token: default);

                return bytes ?? Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read bytes from FTP file: {Path}", filePath);
                InvalidateClient(filePath);

                return Array.Empty<byte>();
            }
        });
    }

    /// <summary>
    /// Saves raw bytes to a file on an FTP server.
    /// Creates parent directories if needed.
    /// </summary>
    public Task SaveBytes(string filePath, byte[] bytes)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (client, remotePath) = await GetFtpClientForPath(filePath);
                var dir = remotePath[..remotePath.LastIndexOf('/')];

                if (!string.IsNullOrEmpty(dir))
                {
                    await client.CreateDirectory(dir, true);
                }

                var status = await client.UploadBytes(bytes, remotePath, FtpRemoteExists.Overwrite, true);

                if (status == FtpStatus.Failed)
                {
                    _logger.LogError("Failed to upload bytes to FTP file: {Path}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save bytes to FTP file: {Path}", filePath);
                InvalidateClient(filePath);
            }
        });
    }

    /// <summary>
    /// Deletes a file from an FTP server.
    /// </summary>
    public Task DeleteFile(string filePath)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (client, remotePath) = await GetFtpClientForPath(filePath);

                if (await client.FileExists(remotePath))
                {
                    await client.DeleteFile(remotePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete FTP file: {Path}", filePath);
                InvalidateClient(filePath);
            }
        });
    }

    /// <summary>
    /// Deletes all files matching the search pattern from an FTP folder.
    /// </summary>
    public Task DeleteFiles(string folderPath, string? searchPattern = null)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (client, remotePath) = await GetFtpClientForPath(folderPath);

                if (!await client.DirectoryExists(remotePath))
                {
                    return;
                }

                var listing = await client.GetListing(remotePath);

                foreach (var item in listing.Where(i => i.Type == FtpObjectType.File))
                {
                    if (MatchesPattern(item.Name, searchPattern))
                    {
                        try
                        {
                            await client.DeleteFile(item.FullName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete FTP file: {File}", item.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete FTP files from: {FolderPath} with pattern: {SearchPattern}", folderPath, searchPattern);
                InvalidateClient(folderPath);
            }
        });
    }

    /// <summary>
    /// Gets all subdirectories matching the search pattern from an FTP folder.
    /// </summary>
    public Task<IEnumerable<string>> GetSubFolders(string folderPath, string? searchPattern = null)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (host, port, user, pass, remotePath) = ParseFtpUri(folderPath);
                var client = await GetOrConnectFtpClient(host, port, user, pass);

                if (!await client.DirectoryExists(remotePath))
                {
                    return Enumerable.Empty<string>();
                }

                var listing = await client.GetListing(remotePath);
                IEnumerable<FtpListItem> dirs = listing.Where(i => i.Type == FtpObjectType.Directory);

                if (!string.IsNullOrEmpty(searchPattern))
                {
                    dirs = dirs.Where(d => MatchesPattern(d.Name, searchPattern));
                }

                return dirs.Select(d => ToFtpFullPath(host, port, user, pass, d.FullName)).ToList().AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get FTP subdirectories from: {FolderPath}", folderPath);
                InvalidateClient(folderPath);

                return Enumerable.Empty<string>();
            }
        });
    }

    /// <summary>
    /// Gets all files matching the search pattern from an FTP folder.
    /// </summary>
    public Task<IEnumerable<string>> GetFiles(string folderPath, string? searchPattern = null)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (host, port, user, pass, remotePath) = ParseFtpUri(folderPath);
                var client = await GetOrConnectFtpClient(host, port, user, pass);

                if (!await client.DirectoryExists(remotePath))
                {
                    return Enumerable.Empty<string>();
                }

                var listing = await client.GetListing(remotePath);
                IEnumerable<FtpListItem> files = listing.Where(i => i.Type == FtpObjectType.File);

                if (!string.IsNullOrEmpty(searchPattern))
                {
                    files = files.Where(f => MatchesPattern(f.Name, searchPattern));
                }

                return files.Select(f => ToFtpFullPath(host, port, user, pass, f.FullName)).ToList().AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get FTP files from: {FolderPath}", folderPath);
                InvalidateClient(folderPath);

                return Enumerable.Empty<string>();
            }
        });
    }

    /// <summary>
    /// Creates a directory on an FTP server.
    /// </summary>
    public Task CreateDirectory(string folderPath)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (client, remotePath) = await GetFtpClientForPath(folderPath);
                await client.CreateDirectory(remotePath, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create FTP directory: {FolderPath}", folderPath);
                InvalidateClient(folderPath);
            }
        });
    }

    /// <summary>
    /// Checks if a file exists on an FTP server.
    /// </summary>
    public Task<bool> FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return Task.FromResult(false);
        }

        return RunFtpOperation(async () =>
        {
            try
            {
                var (client, remotePath) = await GetFtpClientForPath(filePath);

                return await client.FileExists(remotePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if FTP file exists: {Path}", filePath);
                InvalidateClient(filePath);

                return false;
            }
        });
    }

    /// <summary>
    /// Gets the folder name from an FTP path.
    /// </summary>
    public string GetFolderName(string folderPath)
    {
        try
        {
            var trimmed = folderPath.TrimEnd('/');
            var lastSep = trimmed.LastIndexOf('/');

            return lastSep >= 0 ? trimmed[(lastSep + 1)..] : trimmed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get folder name from FTP path: {FolderPath}", folderPath);
            return string.Empty;
        }
    }

    /// <summary>
    /// Combines path segments using the FTP forward slash separator.
    /// </summary>
    public string CombinePath(string basePath, params string[] segments)
    {
        return FtpCombinePaths(basePath, segments);
    }

    /// <summary>
    /// Copies a file within FTP (download then upload).
    /// </summary>
    public Task CopyFile(string source, string destination)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (srcClient, srcRemotePath) = await GetFtpClientForPath(source);

                if (!await srcClient.FileExists(srcRemotePath))
                {
                    _logger.LogWarning("Cannot copy file: FTP source does not exist at {Source}", source);
                    return;
                }

                var bytes = await srcClient.DownloadBytes(srcRemotePath, token: default);
                if (bytes == null)
                {
                    return;
                }

                var (dstClient, dstRemotePath) = await GetFtpClientForPath(destination);
                var dstDir = dstRemotePath[..dstRemotePath.LastIndexOf('/')];

                if (!string.IsNullOrEmpty(dstDir))
                {
                    await dstClient.CreateDirectory(dstDir, true);
                }

                await dstClient.UploadBytes(bytes, dstRemotePath, FtpRemoteExists.Overwrite, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy FTP file from {Source} to {Destination}", source, destination);
                InvalidateClient(source);
            }
        });
    }

    /// <summary>
    /// Moves a file within FTP (copy then delete source).
    /// </summary>
    public Task MoveFile(string source, string destination)
    {
        return RunFtpOperation(async () =>
        {
            try
            {
                var (srcClient, srcRemotePath) = await GetFtpClientForPath(source);

                if (!await srcClient.FileExists(srcRemotePath))
                {
                    _logger.LogWarning("Cannot move file: FTP source does not exist at {Source}", source);
                    return;
                }

                var bytes = await srcClient.DownloadBytes(srcRemotePath, token: default);
                if (bytes == null)
                {
                    return;
                }

                var (dstClient, dstRemotePath) = await GetFtpClientForPath(destination);
                var dstDir = dstRemotePath[..dstRemotePath.LastIndexOf('/')];

                if (!string.IsNullOrEmpty(dstDir))
                {
                    await dstClient.CreateDirectory(dstDir, true);
                }

                await dstClient.UploadBytes(bytes, dstRemotePath, FtpRemoteExists.Overwrite, true);
                await srcClient.DeleteFile(srcRemotePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move FTP file from {Source} to {Destination}", source, destination);
                InvalidateClient(source);
            }
        });
    }

    /// <summary>
    /// Returns the path unchanged — FTP does not rewrite paths.
    /// </summary>
    public Task<string?> CheckPath(string? path) => Task.FromResult(path);

    /// <summary>
    /// Disconnects all cached FTP client connections.
    /// </summary>
    public Task Cleanup()
    {
        return RunFtpOperation(async () =>
        {
            foreach (var kvp in _ftpClients)
            {
                try
                {
                    if (kvp.Value.IsConnected)
                    {
                        await kvp.Value.Disconnect();
                        _logger.LogInformation("Disconnected FTP client: {Key}", kvp.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to disconnect FTP client: {Key}", kvp.Key);
                }
            }

            _ftpClients.Clear();
        });
    }

    #endregion
}
