using iiSUMediaScraper.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Orchestrates file operations by delegating to protocol-specific IFileHandler implementations.
/// Handles cross-protocol transfers, JSON serialization, and temporary file management.
/// </summary>
public class FileService : IFileService
{
    private readonly List<IFileHandler> _handlers;
    private readonly ILogger<FileService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="handlers">The registered file handlers, checked in registration order.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    public FileService(IEnumerable<IFileHandler> handlers, ILogger<FileService> logger)
    {
        _handlers = handlers.ToList();
        _logger = logger;
    }

    #region Private Helpers

    private IFileHandler GetHandler(string? path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            foreach (var handler in _handlers)
            {
                if (handler.CanHandle(path))
                {
                    return handler;
                }
            }
        }

        // Last registered handler is the fallback (CanHandle returns true for all paths)
        return _handlers.Last();
    }

    #endregion

    #region Read Operations

    /// <summary>
    /// Reads and deserializes a JSON file into the specified type.
    /// </summary>
    public async Task<T> Read<T>(string folderPath, string fileName)
    {
        var handler = GetHandler(folderPath);
        var fullPath = handler.CombinePath(folderPath, fileName);
        try
        {
            var bytes = await handler.ReadBytes(fullPath);

            if (bytes.Length == 0)
            {
                return default;
            }

            // Skip UTF-8 BOM if present — JsonSerializer.Deserialize(byte[]) doesn't handle it
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                bytes = bytes[3..];
            }

            return JsonSerializer.Deserialize<T>(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file: {Path}", fullPath);

            return default;
        }
    }

    /// <summary>
    /// Reads a file as a byte array.
    /// </summary>
    public async Task<byte[]> ReadBytes(string folderPath, string fileName)
    {
        var handler = GetHandler(folderPath);
        var fullPath = handler.CombinePath(folderPath, fileName);

        return await handler.ReadBytes(fullPath);
    }

    /// <summary>
    /// Reads a file as a byte array using the full file path.
    /// </summary>
    public async Task<byte[]> ReadBytes(string filePath)
    {
        var handler = GetHandler(filePath);

        return await handler.ReadBytes(filePath);
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Serializes an object to JSON and saves it to a file.
    /// </summary>
    public async Task Save<T>(string folderPath, string fileName, T content)
    {
        var handler = GetHandler(folderPath);
        var fullPath = handler.CombinePath(folderPath, fileName);
        try
        {
            await handler.CreateDirectory(folderPath);
            var fileContent = JsonSerializer.Serialize(content, new JsonSerializerOptions() { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(fileContent);
            await handler.SaveBytes(fullPath, bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {Path}", fullPath);
        }
    }

    /// <summary>
    /// Saves a byte array to a file.
    /// </summary>
    public async Task SaveBytes(string folderPath, string fileName, byte[] bytes)
    {
        var handler = GetHandler(folderPath);
        var fullPath = handler.CombinePath(folderPath, fileName);
        try
        {
            await handler.CreateDirectory(folderPath);
            await handler.SaveBytes(fullPath, bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save bytes to file: {Path}", fullPath);
        }
    }

    #endregion

    #region Delete Operations

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    public async Task Delete(string folderPath, string fileName)
    {
        var handler = GetHandler(folderPath);
        var fullPath = handler.CombinePath(folderPath, fileName);
        await handler.DeleteFile(fullPath);
    }

    /// <summary>
    /// Deletes all files in a folder matching the search pattern.
    /// </summary>
    public async Task DeleteFiles(string folderPath, string searchPattern = null)
    {
        var handler = GetHandler(folderPath);
        await handler.DeleteFiles(folderPath, searchPattern);
    }

    #endregion

    #region Directory & File Enumeration

    /// <summary>
    /// Gets all subdirectories in a folder.
    /// </summary>
    public async Task<IEnumerable<string>> GetSubFolders(string folderPath, string searchPattern = null)
    {
        var handler = GetHandler(folderPath);

        return await handler.GetSubFolders(folderPath, searchPattern);
    }

    /// <summary>
    /// Gets all files in a folder.
    /// </summary>
    public async Task<IEnumerable<string>> GetFiles(string folderPath, string searchPattern = null)
    {
        var handler = GetHandler(folderPath);

        return await handler.GetFiles(folderPath, searchPattern);
    }

    #endregion

    #region Path Helpers

    /// <summary>
    /// Extracts the folder name from a full folder path.
    /// </summary>
    public string GetFolderName(string folderPath)
    {
        var handler = GetHandler(folderPath);

        return handler.GetFolderName(folderPath);
    }

    /// <summary>
    /// Extracts the file name from a full file path.
    /// </summary>
    public string GetFileName(string filePath)
    {
        try
        {
            return Path.GetFileName(filePath) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get file name from path: {FilePath}", filePath);

            return string.Empty;
        }
    }

    /// <summary>
    /// Extracts the file name without extension from a full file path.
    /// </summary>
    public string GetFileNameWithoutExtension(string filePath)
    {
        try
        {
            return Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get file name without extension from path: {FilePath}", filePath);

            return string.Empty;
        }
    }

    /// <summary>
    /// Extracts the extension from a full file path.
    /// </summary>
    public string GetExtension(string filePath)
    {
        try
        {
            return Path.GetExtension(filePath) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get file name without extension from path: {FilePath}", filePath);

            return string.Empty;
        }
    }

    /// <summary>
    /// Removes all invalid file name characters from a string.
    /// </summary>
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

            string pattern = "[" + Regex.Escape(new string(invalidChars)) + "]";

            return Regex.Replace(filename, pattern, "");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean file name: {Filename}", filename);

            return filename ?? string.Empty;
        }
    }

    /// <summary>
    /// Combines multiple path segments into a single path.
    /// </summary>
    public string CombinePath(params string[] paths)
    {
        try
        {
            var validPaths = paths.Where(p => !string.IsNullOrEmpty(p)).ToArray();

            if (validPaths.Length == 0)
            {
                return string.Empty;
            }

            var handler = GetHandler(validPaths[0]);

            if (validPaths.Length == 1)
            {
                return validPaths[0];
            }

            return handler.CombinePath(validPaths[0], validPaths[1..]);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to combine paths");

            return string.Empty;
        }
    }

    #endregion

    #region File Move & Copy

    /// <summary>
    /// Moves a file from source to destination.
    /// Supports cross-protocol transfers by reading bytes from source and writing to destination.
    /// </summary>
    public async Task MoveFile(string source, string desination)
    {
        try
        {
            var srcHandler = GetHandler(source);
            var dstHandler = GetHandler(desination);

            if (ReferenceEquals(srcHandler, dstHandler))
            {
                await srcHandler.MoveFile(source, desination);
            }
            else
            {
                await CopyFile(source, desination);
                await srcHandler.DeleteFile(source);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move file from {Source} to {Destination}", source, desination);
        }
    }

    /// <summary>
    /// Copies a file from source to destination.
    /// Supports cross-protocol transfers by reading bytes from source and writing to destination.
    /// </summary>
    public async Task CopyFile(string source, string destination)
    {
        try
        {
            var srcHandler = GetHandler(source);
            var dstHandler = GetHandler(destination);

            if (ReferenceEquals(srcHandler, dstHandler))
            {
                await srcHandler.CopyFile(source, destination);
            }
            else
            {
                var bytes = await srcHandler.ReadBytes(source);

                if (bytes.Length == 0)
                {
                    _logger.LogWarning("Cannot copy file: source does not exist or is empty at {Source}", source);
                    return;
                }

                await dstHandler.SaveBytes(destination, bytes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy file from {Source} to {Destination}", source, destination);
        }
    }

    #endregion

    #region Directory Operations

    /// <summary>
    /// Creates a directory at the specified path.
    /// </summary>
    public async Task CreateDirectory(string folderPath)
    {
        var handler = GetHandler(folderPath);
        await handler.CreateDirectory(folderPath);
    }

    #endregion

    #region Temporary Files

    /// <summary>
    /// Creates a temporary file in the application's temp folder.
    /// </summary>
    public Task<string> CreateTemporaryFile()
    {
        try
        {
            var tempFolder = GetTemporaryFolder();

            Directory.CreateDirectory(tempFolder);

            var systemTempFile = Path.GetTempFileName();
            var appTempFile = Path.Join(tempFolder, Path.GetFileName(systemTempFile));

            File.Move(systemTempFile, appTempFile);

            return Task.FromResult(appTempFile);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create temp file in app folder, falling back to system temp");

            var fallbackPath = Path.GetTempFileName();

            return Task.FromResult(fallbackPath);
        }
    }

    /// <summary>
    /// Gets a unique temporary file path without creating the file.
    /// </summary>
    public string GetTemporaryFilePath(string? extension = null)
    {
        try
        {
            var tempFolder = GetTemporaryFolder();

            Directory.CreateDirectory(tempFolder);

            var randomName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var fileName = extension != null
                ? $"{randomName}.{extension}"
                : randomName;

            return Path.Join(tempFolder, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get temp file path in app folder, falling back to system temp");

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
    public string GetTemporaryFolder()
    {
        return Path.Join(Path.GetTempPath(), nameof(iiSUMediaScraper));
    }

    /// <summary>
    /// Removes all temporary files and disconnects all handler connections.
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
                    _logger.LogWarning(ex, "Failed to delete temporary file: {File}", file);
                }
            }
        }

        foreach (var handler in _handlers)
        {
            await handler.Cleanup();
        }
    }

    #endregion

    #region Handler Delegation

    /// <summary>
    /// Gives each handler a chance to rewrite a path for its protocol if needed.
    /// Returns the path unchanged if no handler claims it.
    /// </summary>
    public async Task<string?> CheckPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        foreach (var handler in _handlers)
        {
            var result = await handler.CheckPath(path);

            if (result != path)
            {
                return result;
            }
        }

        return path;
    }

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    public async Task<bool> FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        var handler = GetHandler(filePath);

        return await handler.FileExists(filePath);
    }

    #endregion
}
