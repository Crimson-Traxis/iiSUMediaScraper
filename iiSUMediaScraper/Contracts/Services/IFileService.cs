namespace iiSUMediaScraper.Contracts.Services;

public interface IFileService
{
    Task<T> Read<T>(string folderPath, string fileName);

    Task<byte[]> ReadBytes(string folderPath, string fileName);

    Task<byte[]> ReadBytes(string filePath);

    Task Save<T>(string folderPath, string fileName, T content);

    Task SaveBytes(string folderPath, string fileName, byte[] bytes);

    Task Delete(string folderPath, string fileName);

    Task<IEnumerable<string>> GetSubFolders(string folderPath, string searchPattern = null);

    Task<IEnumerable<string>> GetFiles(string folderPath, string searchPattern = null);

    Task DeleteFiles(string folderPath, string searchPattern = null);

    Task MoveFile(string source, string desination);

    Task CreateDirectory(string folderPath);

    string CombinePath(params string[] paths);

    string GetFolderName(string folderPath);

    string GetFileName(string filePath);

    string GetFileNameWithoutExtension(string filePath);

    string CleanFileName(string filePath);
}
