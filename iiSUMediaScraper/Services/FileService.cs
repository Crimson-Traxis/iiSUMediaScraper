using iiSUMediaScraper.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Provides file system operations for reading, writing, and managing files and directories.
/// </summary>
public class FileService : IFileService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    public FileService(ILogger<FileService> logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Reads and deserializes a JSON file into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON content into.</typeparam>
    /// <param name="folderPath">The folder containing the file.</param>
    /// <param name="fileName">The name of the file to read.</param>
    /// <returns>The deserialized object, or default if the file doesn't exist or an error occurs.</returns>
    public async Task<T> Read<T>(string folderPath, string fileName)
    {
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
    /// Deletes a file if it exists.
    /// </summary>
    /// <param name="folderPath">The folder containing the file.</param>
    /// <param name="fileName">The name of the file to delete.</param>
    public Task Delete(string folderPath, string fileName)
    {
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

        return Task.CompletedTask;
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

    /// <summary>
    /// Gets all subdirectories in a folder.
    /// </summary>
    /// <param name="folderPath">The folder to search.</param>
    /// <param name="searchPattern">Optional search pattern (e.g., "*temp*").</param>
    /// <returns>Enumerable of subdirectory paths, or empty if an error occurs.</returns>
    public Task<IEnumerable<string>> GetSubFolders(string folderPath, string searchPattern = null)
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
            Logger.LogError(ex, "Failed to get subdirectories from: {FolderPath}", folderPath);
            return Task.FromResult<IEnumerable<string>>([]);
        }
    }

    /// <summary>
    /// Gets all files in a folder.
    /// </summary>
    /// <param name="folderPath">The folder to search.</param>
    /// <param name="searchPattern">Optional search pattern (e.g., "*.png").</param>
    /// <returns>Enumerable of file paths, or empty if an error occurs.</returns>
    public Task<IEnumerable<string>> GetFiles(string folderPath, string searchPattern = null)
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
            Logger.LogError(ex, "Failed to get files from: {FolderPath}", folderPath);
            return Task.FromResult<IEnumerable<string>>([]);
        }
    }

    /// <summary>
    /// Extracts the folder name from a full folder path.
    /// </summary>
    /// <param name="folderPath">The full folder path.</param>
    /// <returns>The name of the folder, or empty string if an error occurs.</returns>
    public string GetFolderName(string folderPath)
    {
        try
        {
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
    /// Moves a file from source to destination.
    /// Logs an error if the operation fails but does not throw.
    /// </summary>
    /// <param name="source">The source file path.</param>
    /// <param name="desination">The destination file path.</param>
    public Task MoveFile(string source, string desination)
    {
        return Task.Run(() =>
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
    }

    /// <summary>
    /// Copies a file from source to destination.
    /// Logs an error if the operation fails but does not throw.
    /// </summary>
    /// <param name="source">The source file path.</param>
    /// <param name="destination">The destination file path.</param>
    public Task CopyFile(string source, string destination)
    {
        return Task.Run(() =>
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
            return validPaths.Length > 0 ? Path.Combine(validPaths) : string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to combine paths");
            return string.Empty;
        }
    }

    /// <summary>
    /// Deletes all files in a folder matching the search pattern.
    /// Continues deleting remaining files if individual deletions fail.
    /// </summary>
    /// <param name="folderPath">The folder containing files to delete.</param>
    /// <param name="searchPattern">Optional search pattern (e.g., "*.tmp").</param>
    public Task DeleteFiles(string folderPath, string searchPattern = null)
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

    /// <summary>
    /// Creates a directory at the specified path.
    /// Creates all necessary parent directories if they don't exist.
    /// </summary>
    /// <param name="folderPath">The path of the directory to create.</param>
    public Task CreateDirectory(string folderPath)
    {
        try
        {
            Directory.CreateDirectory(folderPath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create directory: {FolderPath}", folderPath);
        }
        return Task.CompletedTask;
    }

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
    /// Removes all the temporary files created by the application.
    /// </summary>
    public Task CleanupTemporaryFiles()
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

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the logger instance for diagnostic output.
    /// </summary>
    protected ILogger Logger { get; private set; }
}
