using iiSUMediaScraper.Contracts.Services;
using MediaDevices;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Enumeration;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Provides file system operations for reading, writing, and managing files and directories.
/// Transparently supports MTP devices via the mtp:\ path prefix.
/// All MTP COM operations are dispatched to a dedicated STA background thread.
/// </summary>
public class FileService : IFileService
{
    private const string MtpPrefix = @"mtp:\";
    private readonly ConcurrentDictionary<string, MediaDevice> _connectedDevices = new(StringComparer.OrdinalIgnoreCase);
    private readonly BlockingCollection<Action> _mtpWorkQueue = new();
    private readonly Thread _mtpThread;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    public FileService(ILogger<FileService> logger)
    {
        Logger = logger;
        _mtpThread = new Thread(MtpThreadLoop)
        {
            IsBackground = true,
            Name = "MTP Worker"
        };
        _mtpThread.SetApartmentState(ApartmentState.STA);
        _mtpThread.Start();
    }

    #region Private MTP Helpers

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

    private bool IsMtpPathInternal(string? path) =>
        !string.IsNullOrEmpty(path) && path.StartsWith(MtpPrefix, StringComparison.OrdinalIgnoreCase);

    private (string deviceName, string mtpPath) ParseMtpPath(string path)
    {
        var remainder = path[MtpPrefix.Length..];
        var separatorIndex = remainder.IndexOf('\\');
        if (separatorIndex < 0)
            return (remainder, @"\");

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
            return cached;

        var devices = MediaDevice.GetDevices();
        var device = devices.FirstOrDefault(d =>
            string.Equals(d.FriendlyName, deviceName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(d.Description, deviceName, StringComparison.OrdinalIgnoreCase));

        if (device == null)
            throw new InvalidOperationException($"MTP device '{deviceName}' not found. Ensure it is connected and in MTP/File Transfer mode.");

        device.Connect();
        _connectedDevices[deviceName] = device;
        Logger.LogInformation("Connected to MTP device: {DeviceName}", deviceName);
        return device;
    }

    private void MtpCreateDirectoryRecursive(MediaDevice device, string mtpPath)
    {
        if (device.DirectoryExists(mtpPath))
            return;

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
            return true;
        return FileSystemName.MatchesSimpleExpression(pattern, fileName);
    }

    #endregion

    #region Read Operations

    /// <summary>
    /// Reads and deserializes a JSON file into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON content into.</typeparam>
    /// <param name="folderPath">The folder containing the file.</param>
    /// <param name="fileName">The name of the file to read.</param>
    /// <returns>The deserialized object, or default if the file doesn't exist or an error occurs.</returns>
    public async Task<T> Read<T>(string folderPath, string fileName)
    {
        if (IsMtpPathInternal(folderPath))
        {
            var fullPath = MtpCombinePaths(folderPath, fileName);
            var (deviceName, mtpPath) = ParseMtpPath(fullPath);
            try
            {
                return await RunOnMtpThread(() =>
                {
                    var device = GetOrConnectDevice(deviceName);
                    if (!device.FileExists(mtpPath))
                        return default;

                    using var ms = new MemoryStream();
                    device.DownloadFile(mtpPath, ms);
                    ms.Position = 0;
                    return JsonSerializer.Deserialize<T>(ms);
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to read MTP file: {Path}", fullPath);
                return default;
            }
        }

        string path = Path.Combine(folderPath, fileName);
        try
        {
            if (File.Exists(path))
            {
                string json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<T>(json);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read file: {Path}", path);
        }

        return default;
    }

    /// <summary>
    /// Reads a file as a byte array.
    /// </summary>
    /// <param name="folderPath">The folder containing the file.</param>
    /// <param name="fileName">The name of the file to read.</param>
    /// <returns>The file content as a byte array, or an empty array if the file doesn't exist or an error occurs.</returns>
    public async Task<byte[]> ReadBytes(string folderPath, string fileName)
    {
        if (IsMtpPathInternal(folderPath))
        {
            var fullPath = MtpCombinePaths(folderPath, fileName);
            return await MtpReadBytes(fullPath);
        }

        string path = Path.Combine(folderPath, fileName);
        try
        {
            if (File.Exists(path))
            {
                return await File.ReadAllBytesAsync(path);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read bytes from file: {Path}", path);
        }
        return [];
    }

    /// <summary>
    /// Reads a file as a byte array using the full file path.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <returns>The file content as a byte array, or an empty array if the file doesn't exist or an error occurs.</returns>
    public async Task<byte[]> ReadBytes(string filePath)
    {
        if (IsMtpPathInternal(filePath))
        {
            return await MtpReadBytes(filePath);
        }

        try
        {
            if (File.Exists(filePath))
            {
                return await File.ReadAllBytesAsync(filePath);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read bytes from file: {FilePath}", filePath);
        }
        return [];
    }

    private async Task<byte[]> MtpReadBytes(string mtpFullPath)
    {
        var (deviceName, mtpPath) = ParseMtpPath(mtpFullPath);
        try
        {
            return await RunOnMtpThread(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                if (!device.FileExists(mtpPath))
                    return Array.Empty<byte>();

                using var ms = new MemoryStream();
                device.DownloadFile(mtpPath, ms);
                return ms.ToArray();
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read bytes from MTP file: {Path}", mtpFullPath);
            return [];
        }
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Serializes an object to JSON and saves it to a file.
    /// Creates the directory if it doesn't exist.
    /// Logs an error if the operation fails but does not throw.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="folderPath">The folder to save the file in.</param>
    /// <param name="fileName">The name of the file to create.</param>
    /// <param name="content">The object to serialize and save.</param>
    public async Task Save<T>(string folderPath, string fileName, T content)
    {
        if (IsMtpPathInternal(folderPath))
        {
            var fullPath = MtpCombinePaths(folderPath, fileName);
            var (deviceName, mtpPath) = ParseMtpPath(fullPath);
            var (_, mtpFolder) = ParseMtpPath(folderPath);
            try
            {
                await RunOnMtpThread(() =>
                {
                    var device = GetOrConnectDevice(deviceName);
                    MtpCreateDirectoryRecursive(device, mtpFolder);

                    var fileContent = JsonSerializer.Serialize(content, new JsonSerializerOptions() { WriteIndented = true });
                    using var ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
                    device.UploadFile(ms, mtpPath);
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save MTP file: {Path}", fullPath);
            }
            return;
        }

        string path = Path.Combine(folderPath, fileName);
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileContent = JsonSerializer.Serialize(content, new JsonSerializerOptions() { WriteIndented = true });
            await File.WriteAllTextAsync(path, fileContent, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save file: {Path}", path);
        }
    }

    /// <summary>
    /// Saves a byte array to a file.
    /// Creates the directory if it doesn't exist.
    /// Logs an error if the operation fails but does not throw.
    /// </summary>
    /// <param name="folderPath">The folder to save the file in.</param>
    /// <param name="fileName">The name of the file to create.</param>
    /// <param name="bytes">The byte array to save.</param>
    public async Task SaveBytes(string folderPath, string fileName, byte[] bytes)
    {
        if (IsMtpPathInternal(folderPath))
        {
            var fullPath = MtpCombinePaths(folderPath, fileName);
            var (deviceName, mtpPath) = ParseMtpPath(fullPath);
            var (_, mtpFolder) = ParseMtpPath(folderPath);
            try
            {
                await RunOnMtpThread(() =>
                {
                    var device = GetOrConnectDevice(deviceName);
                    MtpCreateDirectoryRecursive(device, mtpFolder);

                    using var ms = new MemoryStream(bytes);
                    device.UploadFile(ms, mtpPath);
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save bytes to MTP file: {Path}", fullPath);
            }
            return;
        }

        string path = Path.Combine(folderPath, fileName);
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            await File.WriteAllBytesAsync(path, bytes);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save bytes to file: {Path}", path);
        }
    }

    #endregion

    #region Delete Operations

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    /// <param name="folderPath">The folder containing the file.</param>
    /// <param name="fileName">The name of the file to delete.</param>
    public async Task Delete(string folderPath, string fileName)
    {
        if (IsMtpPathInternal(folderPath))
        {
            var fullPath = MtpCombinePaths(folderPath, fileName);
            var (deviceName, mtpPath) = ParseMtpPath(fullPath);
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
                Logger.LogError(ex, "Failed to delete MTP file: {Path}", fullPath);
            }
            return;
        }

        string path = Path.Combine(folderPath, fileName);
        try
        {
            if (fileName != null && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete file: {Path}", path);
        }
    }

    /// <summary>
    /// Deletes all files in a folder matching the search pattern.
    /// Continues deleting remaining files if individual deletions fail.
    /// </summary>
    /// <param name="folderPath">The folder containing files to delete.</param>
    /// <param name="searchPattern">Optional search pattern (e.g., "*.tmp").</param>
    public async Task DeleteFiles(string folderPath, string searchPattern = null)
    {
        if (IsMtpPathInternal(folderPath))
        {
            var (deviceName, mtpPath) = ParseMtpPath(folderPath);
            try
            {
                await RunOnMtpThread(() =>
                {
                    var device = GetOrConnectDevice(deviceName);
                    if (!device.DirectoryExists(mtpPath))
                        return;

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
                                Logger.LogWarning(ex, "Failed to delete MTP file: {File}", file);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to delete MTP files from: {FolderPath} with pattern: {SearchPattern}", folderPath, searchPattern);
            }
            return;
        }

        await Task.Run(() =>
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
                        Logger.LogWarning(ex, "Failed to delete file: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to enumerate files from: {FolderPath} with pattern: {SearchPattern}", folderPath, searchPattern);
            }
        });
    }

    #endregion

    #region Directory & File Enumeration

    /// <summary>
    /// Gets all subdirectories in a folder.
    /// </summary>
    /// <param name="folderPath">The folder to search.</param>
    /// <param name="searchPattern">Optional search pattern (e.g., "*temp*").</param>
    /// <returns>Enumerable of subdirectory paths, or empty if an error occurs.</returns>
    public async Task<IEnumerable<string>> GetSubFolders(string folderPath, string searchPattern = null)
    {
        if (IsMtpPathInternal(folderPath))
        {
            var (deviceName, mtpPath) = ParseMtpPath(folderPath);
            try
            {
                return await RunOnMtpThread<IEnumerable<string>>(() =>
                {
                    var device = GetOrConnectDevice(deviceName);
                    if (!device.DirectoryExists(mtpPath))
                        return [];

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
                Logger.LogError(ex, "Failed to get MTP subdirectories from: {FolderPath}", folderPath);
                return [];
            }
        }

        try
        {
            IEnumerable<string> result = searchPattern == null
                ? Directory.EnumerateDirectories(folderPath)
                : Directory.EnumerateDirectories(folderPath, searchPattern);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get subdirectories from: {FolderPath}", folderPath);
            return [];
        }
    }

    /// <summary>
    /// Gets all files in a folder.
    /// </summary>
    /// <param name="folderPath">The folder to search.</param>
    /// <param name="searchPattern">Optional search pattern (e.g., "*.png").</param>
    /// <returns>Enumerable of file paths, or empty if an error occurs.</returns>
    public async Task<IEnumerable<string>> GetFiles(string folderPath, string searchPattern = null)
    {
        if (IsMtpPathInternal(folderPath))
        {
            var (deviceName, mtpPath) = ParseMtpPath(folderPath);
            try
            {
                return await RunOnMtpThread<IEnumerable<string>>(() =>
                {
                    var device = GetOrConnectDevice(deviceName);
                    if (!device.DirectoryExists(mtpPath))
                        return [];

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
                Logger.LogError(ex, "Failed to get MTP files from: {FolderPath}", folderPath);
                return [];
            }
        }

        try
        {
            IEnumerable<string> result = searchPattern == null
                ? Directory.EnumerateFiles(folderPath)
                : Directory.EnumerateFiles(folderPath, searchPattern);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get files from: {FolderPath}", folderPath);
            return [];
        }
    }

    #endregion

    #region Path Helpers

    /// <summary>
    /// Extracts the folder name from a full folder path.
    /// </summary>
    /// <param name="folderPath">The full folder path.</param>
    /// <returns>The name of the folder, or empty string if an error occurs.</returns>
    public string GetFolderName(string folderPath)
    {
        try
        {
            if (IsMtpPathInternal(folderPath))
            {
                var trimmed = folderPath.TrimEnd('\\');
                var lastSep = trimmed.LastIndexOf('\\');
                return lastSep >= 0 ? trimmed[(lastSep + 1)..] : trimmed;
            }

            return new DirectoryInfo(folderPath).Name;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get folder name from path: {FolderPath}", folderPath);
            return string.Empty;
        }
    }

    /// <summary>
    /// Extracts the file name from a full file path.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <returns>The name of the file including extension, or empty string if extraction fails.</returns>
    public string GetFileName(string filePath)
    {
        try
        {
            return Path.GetFileName(filePath) ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to get file name from path: {FilePath}", filePath);
            return string.Empty;
        }
    }

    /// <summary>
    /// Extracts the file name without extension from a full file path.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <returns>The name of the file without extension, or empty string if extraction fails.</returns>
    public string GetFileNameWithoutExtension(string filePath)
    {
        try
        {
            return Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to get file name without extension from path: {FilePath}", filePath);
            return string.Empty;
        }
    }

    /// <summary>
    /// Extracts the  extension from a full file path.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <returns>The name of the extension, or empty string if extraction fails.</returns>
    public string GetExtension(string filePath)
    {
        try
        {
            return Path.GetExtension(filePath) ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to get file name without extension from path: {FilePath}", filePath);
            return string.Empty;
        }
    }

    /// <summary>
    /// Removes all invalid file name characters from a string.
    /// </summary>
    /// <param name="filename">The filename to clean.</param>
    /// <returns>The filename with invalid characters removed, or original if cleaning fails.</returns>
    public string CleanFileName(string filename)
    {
        try
        {
            if (string.IsNullOrEmpty(filename))
            {
                return string.Empty;
            }

            char[] invalidChars = Path.GetInvalidPathChars()
                                .Union(Path.GetInvalidFileNameChars())
                                .Distinct()
                                .ToArray();

            // Build regex pattern from all invalid path and filename characters
            string pattern = "[" + Regex.Escape(new string(invalidChars)) + "]";

            // Remove all invalid characters
            return Regex.Replace(filename, pattern, "");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to clean file name: {Filename}", filename);
            return filename ?? string.Empty;
        }
    }

    /// <summary>
    /// Combines multiple path segments into a single path.
    /// Filters out null or empty segments.
    /// </summary>
    /// <param name="paths">The path segments to combine.</param>
    /// <returns>The combined path, or empty string if combination fails.</returns>
    public string CombinePath(params string[] paths)
    {
        try
        {
            var validPaths = paths.Where(p => !string.IsNullOrEmpty(p)).ToArray();
            if (validPaths.Length == 0)
                return string.Empty;

            if (IsMtpPathInternal(validPaths[0]))
            {
                return MtpCombinePaths(validPaths[0], validPaths[1..]);
            }

            return Path.Combine(validPaths);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to combine paths");
            return string.Empty;
        }
    }

    #endregion

    #region File Move & Copy

    /// <summary>
    /// Moves a file from source to destination.
    /// Supports local-to-local, local-to-MTP, MTP-to-local, and MTP-to-MTP transfers.
    /// Logs an error if the operation fails but does not throw.
    /// </summary>
    /// <param name="source">The source file path.</param>
    /// <param name="desination">The destination file path.</param>
    public async Task MoveFile(string source, string desination)
    {
        bool srcMtp = IsMtpPathInternal(source);
        bool dstMtp = IsMtpPathInternal(desination);

        if (!srcMtp && !dstMtp)
        {
            // Local to local — existing behavior
            await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(source))
                    {
                        Logger.LogWarning("Cannot move file: source does not exist at {Source}", source);
                        return;
                    }

                    var destinationDir = Path.GetDirectoryName(desination);
                    if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    File.Move(source, desination, overwrite: true);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to move file from {Source} to {Destination}", source, desination);
                }
            });
            return;
        }

        try
        {
            await RunOnMtpThread(() =>
            {
                if (!srcMtp && dstMtp)
                {
                    // Local to MTP
                    if (!File.Exists(source))
                    {
                        Logger.LogWarning("Cannot move file: source does not exist at {Source}", source);
                        return;
                    }

                    var (dstDevice, dstMtpPath) = ParseMtpPath(desination);
                    var device = GetOrConnectDevice(dstDevice);

                    var dstDir = dstMtpPath[..dstMtpPath.LastIndexOf('\\')];
                    if (!string.IsNullOrEmpty(dstDir))
                        MtpCreateDirectoryRecursive(device, dstDir);

                    using (var stream = File.OpenRead(source))
                    {
                        device.UploadFile(stream, dstMtpPath);
                    }
                    File.Delete(source);
                }
                else if (srcMtp && !dstMtp)
                {
                    // MTP to local
                    var (srcDevice, srcMtpPath) = ParseMtpPath(source);
                    var device = GetOrConnectDevice(srcDevice);

                    if (!device.FileExists(srcMtpPath))
                    {
                        Logger.LogWarning("Cannot move file: MTP source does not exist at {Source}", source);
                        return;
                    }

                    var destinationDir = Path.GetDirectoryName(desination);
                    if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    using var ms = new MemoryStream();
                    device.DownloadFile(srcMtpPath, ms);
                    File.WriteAllBytes(desination, ms.ToArray());
                    device.DeleteFile(srcMtpPath);
                }
                else
                {
                    // MTP to MTP
                    var (srcDevice, srcMtpPath) = ParseMtpPath(source);
                    var (dstDevice, dstMtpPath) = ParseMtpPath(desination);
                    var srcDev = GetOrConnectDevice(srcDevice);

                    if (!srcDev.FileExists(srcMtpPath))
                    {
                        Logger.LogWarning("Cannot move file: MTP source does not exist at {Source}", source);
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
                        MtpCreateDirectoryRecursive(dstDev, dstDir);

                    dstDev.UploadFile(ms, dstMtpPath);
                    srcDev.DeleteFile(srcMtpPath);
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to move file from {Source} to {Destination}", source, desination);
        }
    }

    /// <summary>
    /// Copies a file from source to destination.
    /// Supports local-to-local, local-to-MTP, MTP-to-local, and MTP-to-MTP transfers.
    /// Logs an error if the operation fails but does not throw.
    /// </summary>
    /// <param name="source">The source file path.</param>
    /// <param name="destination">The destination file path.</param>
    public async Task CopyFile(string source, string destination)
    {
        bool srcMtp = IsMtpPathInternal(source);
        bool dstMtp = IsMtpPathInternal(destination);

        if (!srcMtp && !dstMtp)
        {
            // Local to local — existing behavior
            await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(source))
                    {
                        Logger.LogWarning("Cannot copy file: source does not exist at {Source}", source);
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
                    Logger.LogError(ex, "Failed to copy file from {Source} to {Destination}", source, destination);
                }
            });
            return;
        }

        try
        {
            await RunOnMtpThread(() =>
            {
                if (!srcMtp && dstMtp)
                {
                    // Local to MTP
                    if (!File.Exists(source))
                    {
                        Logger.LogWarning("Cannot copy file: source does not exist at {Source}", source);
                        return;
                    }

                    var (dstDevice, dstMtpPath) = ParseMtpPath(destination);
                    var device = GetOrConnectDevice(dstDevice);

                    var dstDir = dstMtpPath[..dstMtpPath.LastIndexOf('\\')];
                    if (!string.IsNullOrEmpty(dstDir))
                        MtpCreateDirectoryRecursive(device, dstDir);

                    if (device.FileExists(dstMtpPath))
                        device.DeleteFile(dstMtpPath);

                    using var stream = File.OpenRead(source);
                    device.UploadFile(stream, dstMtpPath);
                }
                else if (srcMtp && !dstMtp)
                {
                    // MTP to local
                    var (srcDevice, srcMtpPath) = ParseMtpPath(source);
                    var device = GetOrConnectDevice(srcDevice);

                    if (!device.FileExists(srcMtpPath))
                    {
                        Logger.LogWarning("Cannot copy file: MTP source does not exist at {Source}", source);
                        return;
                    }

                    var destinationDir = Path.GetDirectoryName(destination);
                    if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    using var ms = new MemoryStream();
                    device.DownloadFile(srcMtpPath, ms);
                    File.WriteAllBytes(destination, ms.ToArray());
                }
                else
                {
                    // MTP to MTP
                    var (srcDevice, srcMtpPath) = ParseMtpPath(source);
                    var (dstDevice, dstMtpPath) = ParseMtpPath(destination);
                    var srcDev = GetOrConnectDevice(srcDevice);

                    if (!srcDev.FileExists(srcMtpPath))
                    {
                        Logger.LogWarning("Cannot copy file: MTP source does not exist at {Source}", source);
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
                        MtpCreateDirectoryRecursive(dstDev, dstDir);

                    if (dstDev.FileExists(dstMtpPath))
                        dstDev.DeleteFile(dstMtpPath);

                    dstDev.UploadFile(ms, dstMtpPath);
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to copy file from {Source} to {Destination}", source, destination);
        }
    }

    #endregion

    #region Directory Operations

    /// <summary>
    /// Creates a directory at the specified path.
    /// Creates all necessary parent directories if they don't exist.
    /// </summary>
    /// <param name="folderPath">The path of the directory to create.</param>
    public async Task CreateDirectory(string folderPath)
    {
        if (IsMtpPathInternal(folderPath))
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
                Logger.LogError(ex, "Failed to create MTP directory: {FolderPath}", folderPath);
            }
            return;
        }

        try
        {
            Directory.CreateDirectory(folderPath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create directory: {FolderPath}", folderPath);
        }
    }

    #endregion

    #region Temporary Files

    /// <summary>
    /// Creates a temporary file in the application's temp folder.
    /// Uses framework's GetTempFileName for guaranteed uniqueness.
    /// Falls back to system temp if application folder fails.
    /// </summary>
    /// <returns>The path to the created temporary file.</returns>
    public Task<string> CreateTemporaryFile()
    {
        try
        {
            var tempFolder = GetTemporaryFolder();

            Directory.CreateDirectory(tempFolder);

            // Use framework's temp file creation, then move to our folder
            var systemTempFile = Path.GetTempFileName();
            var appTempFile = Path.Join(tempFolder, Path.GetFileName(systemTempFile));

            File.Move(systemTempFile, appTempFile);

            return Task.FromResult(appTempFile);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to create temp file in app folder, falling back to system temp");

            // Fallback to system temp
            var fallbackPath = Path.GetTempFileName();
            return Task.FromResult(fallbackPath);
        }
    }

    /// <summary>
    /// Gets a unique temporary file path without creating the file.
    /// Uses Path.GetRandomFileName() for cryptographically strong random names.
    /// Useful for tools that create their own output files (e.g., yt-dlp).
    /// Falls back to system temp if application folder fails.
    /// </summary>
    /// <param name="extension">Optional file extension (without dot).</param>
    /// <returns>A unique file path in the application's temp folder.</returns>
    public string GetTemporaryFilePath(string? extension = null)
    {
        try
        {
            var tempFolder = GetTemporaryFolder();

            Directory.CreateDirectory(tempFolder);

            // Use framework's random file name generator (cryptographically strong)
            var randomName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var fileName = extension != null
                ? $"{randomName}.{extension}"
                : randomName;

            return Path.Join(tempFolder, fileName);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to get temp file path in app folder, falling back to system temp");

            // Fallback to system temp
            var randomName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var fileName = extension != null
                ? $"{randomName}.{extension}"
                : randomName;

            return Path.Join(Path.GetTempPath(), fileName);
        }
    }

    /// <summary>
    /// Gets the application's temporary folder path.
    /// </summary>
    /// <returns>The path to the application's temp folder.</returns>
    public string GetTemporaryFolder()
    {
        return Path.Join(Path.GetTempPath(), nameof(iiSUMediaScraper));
    }

    /// <summary>
    /// Removes all the temporary files created by the application and disconnects MTP devices.
    /// </summary>
    public async Task CleanupTemporaryFiles()
    {
        var tempFolder = GetTemporaryFolder();

        if (Directory.Exists(tempFolder))
        {
            foreach (var file in Directory.EnumerateFiles(tempFolder))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to delete temporary file: {File}", file);
                }
            }
        }

        await DisconnectMtpDevices();
    }

    #endregion

    #region MTP Public Methods

    /// <summary>
    /// Checks if the first segment of a path matches a connected MTP device
    /// and rewrites it to include the mtp:\ prefix if so.
    /// Returns the path unchanged if it already has the mtp:\ prefix or no device matches.
    /// </summary>
    public async Task<string?> CheckPath(string? path)
    {
        if (string.IsNullOrEmpty(path) || IsMtpPathInternal(path))
            return path;

        var normalized = path.Replace('/', '\\');

        // Strip "This PC\" prefix that File Explorer adds to MTP paths
        if (normalized.StartsWith(@"This PC\", StringComparison.OrdinalIgnoreCase))
            normalized = normalized[@"This PC\".Length..];

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
                Logger.LogInformation("Path matched MTP device '{DeviceName}', rewriting to mtp:\\ prefix", firstSegment);
                return MtpPrefix + normalized;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check path against MTP devices: {Path}", path);
        }

        return path;
    }

    /// <summary>
    /// Returns true if the path refers to an MTP device (uses the mtp:\ prefix).
    /// </summary>
    public bool IsMtpPath(string path) => IsMtpPathInternal(path);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    public async Task<bool> FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        if (IsMtpPathInternal(filePath))
        {
            var (deviceName, mtpPath) = ParseMtpPath(filePath);
            return await RunOnMtpThread(() =>
            {
                var device = GetOrConnectDevice(deviceName);
                return device.FileExists(mtpPath);
            });
        }

        return File.Exists(filePath);
    }

    /// <summary>
    /// Gets the friendly names of all connected MTP devices.
    /// </summary>
    public async Task<IEnumerable<string>> GetMtpDevices()
    {
        try
        {
            return await RunOnMtpThread<IEnumerable<string>>(() =>
                MediaDevice.GetDevices().Select(d => d.FriendlyName).ToList());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to enumerate MTP devices");
            return [];
        }
    }

    /// <summary>
    /// Disconnects all cached MTP device connections.
    /// </summary>
    public async Task DisconnectMtpDevices()
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
                        Logger.LogInformation("Disconnected MTP device: {DeviceName}", kvp.Key);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to disconnect MTP device: {DeviceName}", kvp.Key);
                }
            }
            _connectedDevices.Clear();
        });
    }

    #endregion

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }
}
