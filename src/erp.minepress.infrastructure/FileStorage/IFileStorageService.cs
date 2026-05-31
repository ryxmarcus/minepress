namespace erp.minepress.infrastructure.FileStorage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
