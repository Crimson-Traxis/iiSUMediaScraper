namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Defines file operations that vary by protocol (local, MTP, FTP, etc.).
/// FileService routes to the correct handler based on CanHandle().
/// </summary>
public interface IFileHandler
{
    /// <summary>
    /// Returns true if this handler can process the given path.
    /// </summary>
    bool CanHandle(string? path);

    /// <summary>
    /// Reads a file as raw bytes from a full path.
    /// </summary>
    Task<byte[]> ReadBytes(string filePath);

    /// <summary>
    /// Saves raw bytes to a file. Creates parent directories if needed.
    /// </summary>
    Task SaveBytes(string filePath, byte[] bytes);

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    Task DeleteFile(string filePath);

    /// <summary>
    /// Deletes all files matching the search pattern in a folder.
    /// </summary>
    Task DeleteFiles(string folderPath, string? searchPattern = null);

    /// <summary>
    /// Gets all subdirectories matching the search pattern.
    /// Returns full paths using the handler's protocol prefix.
    /// </summary>
    Task<IEnumerable<string>> GetSubFolders(string folderPath, string? searchPattern = null);

    /// <summary>
    /// Gets all files matching the search pattern.
    /// Returns full paths using the handler's protocol prefix.
    /// </summary>
    Task<IEnumerable<string>> GetFiles(string folderPath, string? searchPattern = null);

    /// <summary>
    /// Creates a directory recursively if it does not exist.
    /// </summary>
    Task CreateDirectory(string folderPath);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    Task<bool> FileExists(string filePath);

    /// <summary>
    /// Gets the folder name (last segment) from a folder path.
    /// </summary>
    string GetFolderName(string folderPath);

    /// <summary>
    /// Combines path segments using the handler's path separator.
    /// </summary>
    string CombinePath(string basePath, params string[] segments);

    /// <summary>
    /// Copies a file within this handler's protocol.
    /// </summary>
    Task CopyFile(string source, string destination);

    /// <summary>
    /// Moves a file within this handler's protocol.
    /// </summary>
    Task MoveFile(string source, string destination);

    /// <summary>
    /// Checks if a path should be rewritten for this handler's protocol.
    /// Returns the path unchanged if no rewriting is needed.
    /// </summary>
    Task<string?> CheckPath(string? path);

    /// <summary>
    /// Disconnects/disposes any cached connections for this handler.
    /// </summary>
    Task Cleanup();
}
