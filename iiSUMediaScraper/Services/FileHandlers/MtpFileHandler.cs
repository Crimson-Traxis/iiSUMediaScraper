using iiSUMediaScraper.Contracts.Services;
using MediaDevices;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Enumeration;

namespace iiSUMediaScraper.Services.FileHandlers;

/// <summary>
/// File handler for MTP (Media Transfer Protocol) devices.
/// All COM operations are dispatched to a dedicated STA background thread.
/// </summary>
public class MtpFileHandler : IFileHandler
{
    private const string MtpPrefix = @"mtp:\";
    private readonly ConcurrentDictionary<string, MediaDevice> _connectedDevices = new(StringComparer.OrdinalIgnoreCase);
    private readonly BlockingCollection<Action> _mtpWorkQueue = new();
    private readonly Thread _mtpThread;
    private readonly ILogger<MtpFileHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MtpFileHandler"/> class.
    /// Starts the dedicated STA thread for MTP COM operations.
    /// </summary>
    public MtpFileHandler(ILogger<MtpFileHandler> logger)
    {
        _logger = logger;
        _mtpThread = new Thread(MtpThreadLoop)
        {
            IsBackground = true,
            Name = "MTP Worker"
        };
        _mtpThread.SetApartmentState(ApartmentState.STA);
        _mtpThread.Start();
    }

    #region Private MTP Threading

    private void MtpThreadLoop()
    {
        foreach (var work in _mtpWorkQueue.GetConsumingEnumerable())
        {
            work();
        }
    }

    private Task RunOnMtpThread(Action action)
    {
        var tcs = new TaskCompletionSource();
        _mtpWorkQueue.Add(() =>
        {
            try
            {
                action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    private Task<T> RunOnMtpThread<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        _mtpWorkQueue.Add(() =>
        {
            try
            {
                tcs.SetResult(func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    #endregion

    #region Private MTP Helpers

    private (string deviceName, string mtpPath) ParseMtpPath(string path)
    {
        var remainder = path[MtpPrefix.Length..];
        var separatorIndex = remainder.IndexOf('\\');

        if (separatorIndex < 0)
        {
            return (remainder, @"\");
        }

        var deviceName = remainder[..separatorIndex];
        var mtpPath = remainder[separatorIndex..];
        return (deviceName, mtpPath);
    }

    private string ToMtpFullPath(string deviceName, string mtpPath)
    {
        return $"{MtpPrefix}{deviceName}{mtpPath}";
    }

    private MediaDevice GetOrConnectDevice(string deviceName)
    {
        if (_connectedDevices.TryGetValue(deviceName, out var cached) && cached.IsConnected)
        {
            return cached;
        }

        var devices = MediaDevice.GetDevices();

        var device = devices.FirstOrDefault(d =>
            string.Equals(d.FriendlyName, deviceName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(d.Description, deviceName, StringComparison.OrdinalIgnoreCase));

        if (device == null)
        {
            throw new InvalidOperationException($"MTP device '{deviceName}' not found. Ensure it is connected and in MTP/File Transfer mode.");
        }

        device.Connect();
        _connectedDevices[deviceName] = device;
        _logger.LogInformation("Connected to MTP device: {DeviceName}", deviceName);

        return device;
    }

    private void MtpCreateDirectoryRecursive(MediaDevice device, string mtpPath)
    {
        if (device.DirectoryExists(mtpPath))
        {
            return;
        }

        var segments = mtpPath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        var current = "";

        foreach (var segment in segments)
        {
            current += @"\" + segment;
            if (!device.DirectoryExists(current))
            {
                device.CreateDirectory(current);
            }
        }
    }

    private string MtpCombinePaths(string basePath, params string[] segments)
    {
        var result = basePath.TrimEnd('\\');

        foreach (var segment in segments.Where(s => !string.IsNullOrEmpty(s)))
        {
            result += @"\" + segment.Trim('\\');
        }

        return result;
    }

    private bool MatchesPattern(string fileName, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern) || pattern == "*")
        {
            return true;
        }

        return FileSystemName.MatchesSimpleExpression(pattern, fileName);
    }

    #endregion

    #region IFileHandler

    /// <summary>
    /// Returns true if the path starts with the mtp:\ prefix.
    /// </summary>
    public bool CanHandle(string? path) =>
        !string.IsNullOrEmpty(path) && path.StartsWith(MtpPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Reads a file as raw bytes from an MTP device.
    /// </summary>
    public async Task<byte[]> ReadBytes(string filePath)
    {
        var (deviceName, mtpPath) = ParseMtpPath(filePath);
        try
        {
            return await RunOnMtpThread(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                if (!device.FileExists(mtpPath))
                {
                    return Array.Empty<byte>();
                }

                using var ms = new MemoryStream();
                device.DownloadFile(mtpPath, ms);
                return ms.ToArray();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read bytes from MTP file: {Path}", filePath);
            return [];
        }
    }

    /// <summary>
    /// Saves raw bytes to a file on an MTP device.
    /// Creates parent directories if needed.
    /// </summary>
    public async Task SaveBytes(string filePath, byte[] bytes)
    {
        var (deviceName, mtpPath) = ParseMtpPath(filePath);
        var mtpFolder = mtpPath[..mtpPath.LastIndexOf('\\')];

        try
        {
            await RunOnMtpThread(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                if (!string.IsNullOrEmpty(mtpFolder))
                {
                    MtpCreateDirectoryRecursive(device, mtpFolder);
                }

                using var ms = new MemoryStream(bytes);

                device.UploadFile(ms, mtpPath);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save bytes to MTP file: {Path}", filePath);
        }
    }

    /// <summary>
    /// Deletes a file from an MTP device.
    /// </summary>
    public async Task DeleteFile(string filePath)
    {
        var (deviceName, mtpPath) = ParseMtpPath(filePath);
        try
        {
            await RunOnMtpThread(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                if (device.FileExists(mtpPath))
                {
                    device.DeleteFile(mtpPath);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete MTP file: {Path}", filePath);
        }
    }

    /// <summary>
    /// Deletes all files matching the search pattern from an MTP folder.
    /// </summary>
    public async Task DeleteFiles(string folderPath, string? searchPattern = null)
    {
        var (deviceName, mtpPath) = ParseMtpPath(folderPath);
        try
        {
            await RunOnMtpThread(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                if (!device.DirectoryExists(mtpPath))
                {
                    return;
                }

                var files = device.GetFiles(mtpPath);

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);

                    if (MatchesPattern(fileName, searchPattern))
                    {
                        try
                        {
                            device.DeleteFile(file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete MTP file: {File}", file);
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete MTP files from: {FolderPath} with pattern: {SearchPattern}", folderPath, searchPattern);
        }
    }

    /// <summary>
    /// Gets all subdirectories matching the search pattern from an MTP folder.
    /// </summary>
    public async Task<IEnumerable<string>> GetSubFolders(string folderPath, string? searchPattern = null)
    {
        var (deviceName, mtpPath) = ParseMtpPath(folderPath);
        try
        {
            return await RunOnMtpThread<IEnumerable<string>>(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                if (!device.DirectoryExists(mtpPath))
                {
                    return [];
                }

                var dirs = device.GetDirectories(mtpPath);
                IEnumerable<string> results = dirs;

                if (!string.IsNullOrEmpty(searchPattern))
                {
                    results = results.Where(d => MatchesPattern(Path.GetFileName(d), searchPattern));
                }

                return results.Select(d => ToMtpFullPath(deviceName, d)).ToList();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MTP subdirectories from: {FolderPath}", folderPath);
            return [];
        }
    }

    /// <summary>
    /// Gets all files matching the search pattern from an MTP folder.
    /// </summary>
    public async Task<IEnumerable<string>> GetFiles(string folderPath, string? searchPattern = null)
    {
        var (deviceName, mtpPath) = ParseMtpPath(folderPath);
        try
        {
            return await RunOnMtpThread<IEnumerable<string>>(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                if (!device.DirectoryExists(mtpPath))
                {
                    return [];
                }

                var files = device.GetFiles(mtpPath);
                IEnumerable<string> results = files;

                if (!string.IsNullOrEmpty(searchPattern))
                {
                    results = results.Where(f => MatchesPattern(Path.GetFileName(f), searchPattern));
                }

                return results.Select(f => ToMtpFullPath(deviceName, f)).ToList();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MTP files from: {FolderPath}", folderPath);
            return [];
        }
    }

    /// <summary>
    /// Creates a directory on an MTP device.
    /// </summary>
    public async Task CreateDirectory(string folderPath)
    {
        var (deviceName, mtpPath) = ParseMtpPath(folderPath);
        try
        {
            await RunOnMtpThread(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                MtpCreateDirectoryRecursive(device, mtpPath);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create MTP directory: {FolderPath}", folderPath);
        }
    }

    /// <summary>
    /// Checks if a file exists on an MTP device.
    /// </summary>
    public async Task<bool> FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        var (deviceName, mtpPath) = ParseMtpPath(filePath);

        return await RunOnMtpThread(() =>
        {
            var device = GetOrConnectDevice(deviceName);

            return device.FileExists(mtpPath);
        });
    }

    /// <summary>
    /// Gets the folder name from an MTP path.
    /// </summary>
    public string GetFolderName(string folderPath)
    {
        try
        {
            var trimmed = folderPath.TrimEnd('\\');
            var lastSep = trimmed.LastIndexOf('\\');

            return lastSep >= 0 ? trimmed[(lastSep + 1)..] : trimmed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get folder name from MTP path: {FolderPath}", folderPath);

            return string.Empty;
        }
    }

    /// <summary>
    /// Combines path segments using the MTP backslash separator.
    /// </summary>
    public string CombinePath(string basePath, params string[] segments)
    {
        return MtpCombinePaths(basePath, segments);
    }

    /// <summary>
    /// Copies a file within MTP (same device or cross-device).
    /// </summary>
    public async Task CopyFile(string source, string destination)
    {
        try
        {
            await RunOnMtpThread(() =>
            {
                var (srcDevice, srcMtpPath) = ParseMtpPath(source);
                var (dstDevice, dstMtpPath) = ParseMtpPath(destination);
                var srcDev = GetOrConnectDevice(srcDevice);

                if (!srcDev.FileExists(srcMtpPath))
                {
                    _logger.LogWarning("Cannot copy file: MTP source does not exist at {Source}", source);
                    return;
                }

                using var ms = new MemoryStream();
                srcDev.DownloadFile(srcMtpPath, ms);
                ms.Position = 0;

                var dstDev = string.Equals(srcDevice, dstDevice, StringComparison.OrdinalIgnoreCase)
                    ? srcDev
                    : GetOrConnectDevice(dstDevice);

                var dstDir = dstMtpPath[..dstMtpPath.LastIndexOf('\\')];

                if (!string.IsNullOrEmpty(dstDir))
                {
                    MtpCreateDirectoryRecursive(dstDev, dstDir);
                }

                if (dstDev.FileExists(dstMtpPath))
                {
                    dstDev.DeleteFile(dstMtpPath);
                }

                dstDev.UploadFile(ms, dstMtpPath);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy MTP file from {Source} to {Destination}", source, destination);
        }
    }

    /// <summary>
    /// Moves a file within MTP (same device or cross-device).
    /// </summary>
    public async Task MoveFile(string source, string destination)
    {
        try
        {
            await RunOnMtpThread(() =>
            {
                var (srcDevice, srcMtpPath) = ParseMtpPath(source);
                var (dstDevice, dstMtpPath) = ParseMtpPath(destination);
                var srcDev = GetOrConnectDevice(srcDevice);

                if (!srcDev.FileExists(srcMtpPath))
                {
                    _logger.LogWarning("Cannot move file: MTP source does not exist at {Source}", source);
                    return;
                }

                using var ms = new MemoryStream();
                srcDev.DownloadFile(srcMtpPath, ms);
                ms.Position = 0;

                var dstDev = string.Equals(srcDevice, dstDevice, StringComparison.OrdinalIgnoreCase)
                    ? srcDev
                    : GetOrConnectDevice(dstDevice);

                var dstDir = dstMtpPath[..dstMtpPath.LastIndexOf('\\')];

                if (!string.IsNullOrEmpty(dstDir))
                {
                    MtpCreateDirectoryRecursive(dstDev, dstDir);
                }

                dstDev.UploadFile(ms, dstMtpPath);
                srcDev.DeleteFile(srcMtpPath);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move MTP file from {Source} to {Destination}", source, destination);
        }
    }

    /// <summary>
    /// Checks if the first segment of a path matches a connected MTP device
    /// and rewrites it to include the mtp:\ prefix if so.
    /// Returns the path unchanged if no device matches.
    /// </summary>
    public async Task<string?> CheckPath(string? path)
    {
        if (string.IsNullOrEmpty(path) || CanHandle(path))
        {
            return path;
        }

        var normalized = path.Replace('/', '\\');

        // Strip "This PC\" prefix that File Explorer adds to MTP paths
        if (normalized.StartsWith(@"This PC\", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[@"This PC\".Length..];
        }

        var separatorIndex = normalized.IndexOf('\\');
        var firstSegment = separatorIndex >= 0 ? normalized[..separatorIndex] : normalized;

        try
        {
            var matched = await RunOnMtpThread(() =>
            {
                var devices = MediaDevice.GetDevices();

                return devices.Any(d =>
                    string.Equals(d.FriendlyName, firstSegment, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d.Description, firstSegment, StringComparison.OrdinalIgnoreCase));
            });

            if (matched)
            {
                _logger.LogInformation("Path matched MTP device '{DeviceName}', rewriting to mtp:\\ prefix", firstSegment);
                return MtpPrefix + normalized;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check path against MTP devices: {Path}", path);
        }

        return path;
    }

    /// <summary>
    /// Disconnects all cached MTP device connections.
    /// </summary>
    public async Task Cleanup()
    {
        await RunOnMtpThread(() =>
        {
            foreach (var kvp in _connectedDevices)
            {
                try
                {
                    if (kvp.Value.IsConnected)
                    {
                        kvp.Value.Disconnect();
                        _logger.LogInformation("Disconnected MTP device: {DeviceName}", kvp.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to disconnect MTP device: {DeviceName}", kvp.Key);
                }
            }

            _connectedDevices.Clear();
        });
    }

    #endregion

}
