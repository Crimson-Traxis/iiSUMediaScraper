namespace iiSUMediaScraper.Contracts.Services;

/// <summary>
/// Service for file system operations including reading, writing, and managing files.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Reads and deserializes a JSON file to the specified type.
    /// </summary>
    Task<T> Read<T>(string folderPath, string fileName);

    /// <summary>
    /// Reads a file as raw bytes from a folder and filename.
    /// </summary>
    Task<byte[]> ReadBytes(string folderPath, string fileName);

    /// <summary>
    /// Reads a file as raw bytes from a full path.
    /// </summary>
    Task<byte[]> ReadBytes(string filePath);

    /// <summary>
    /// Serializes and saves content to a JSON file.
    /// </summary>
    Task Save<T>(string folderPath, string fileName, T content);

    /// <summary>
    /// Saves raw bytes to a file.
    /// </summary>
    Task SaveBytes(string folderPath, string fileName, byte[] bytes);

    /// <summary>
    /// Deletes a file from the specified folder.
    /// </summary>
    Task Delete(string folderPath, string fileName);

    /// <summary>
    /// Deletes a file from the specified file.
    /// </summary>
    Task Delete(string fileName);

    /// <summary>
    /// Gets all subfolders matching the search pattern.
    /// </summary>
    Task<IEnumerable<string>> GetSubFolders(string folderPath, string searchPattern = null);

    /// <summary>
    /// Gets all files matching the search pattern.
    /// </summary>
    Task<IEnumerable<string>> GetFiles(string folderPath, string searchPattern = null);

    /// <summary>
    /// Deletes all files matching the search pattern.
    /// </summary>
    Task DeleteFiles(string folderPath, string searchPattern = null);

    /// <summary>
    /// Moves a file from source to destination.
    /// </summary>
    Task MoveFile(string source, string desination);

    /// <summary>
    /// Copies a file from source to destination.
    /// </summary>
    Task CopyFile(string source, string destination);

    /// <summary>
    /// Creates a directory if it does not exist.
    /// </summary>
    Task CreateDirectory(string folderPath);

    /// <summary>
    /// Creates a temporary file and returns its path.
    /// </summary>
    Task<string> CreateTemporaryFile();

    /// <summary>
    /// Gets a unique temporary file path without creating the file.
    /// Useful for tools that create their own output files (e.g., yt-dlp).
    /// </summary>
    /// <param name="extension">Optional file extension (without dot).</param>
    string GetTemporaryFilePath(string? extension = null);

    /// <summary>
    /// Gets the application's temporary folder path.
    /// </summary>
    string GetTemporaryFolder();

    /// <summary>
    /// Cleans up all temporary files created by this service.
    /// </summary>
    Task CleanupTemporaryFiles();

    /// <summary>
    /// Combines multiple path segments into a single path.
    /// </summary>
    string CombinePath(params string[] paths);

    /// <summary>
    /// Gets the folder name from a folder path.
    /// </summary>
    string GetFolderName(string folderPath);

    /// <summary>
    /// Gets the file name from a file path.
    /// </summary>
    string GetFileName(string filePath);

    /// <summary>
    /// Gets the file name without extension from a file path.
    /// </summary>
    string GetFileNameWithoutExtension(string filePath);

    /// <summary>
    /// Gets the extension from a file path.
    /// </summary>
    string GetExtension(string filePath);

    /// <summary>
    /// Cleans a file name by removing invalid characters.
    /// </summary>
    string CleanFileName(string filePath);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    Task<bool> FileExists(string filePath);

    /// <summary>
    /// Gives each handler a chance to rewrite a path for its protocol if needed.
    /// Returns the path unchanged if no handler claims it.
    /// </summary>
    Task<string?> CheckPath(string? path);
}
