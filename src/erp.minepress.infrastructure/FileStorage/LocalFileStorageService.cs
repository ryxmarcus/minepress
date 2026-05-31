namespace erp.minepress.infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(string basePath = "uploads")
    {
        _basePath = basePath;
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var folderPath = Path.Combine(_basePath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        using var file = File.Create(filePath);
        await fileStream.CopyToAsync(file, cancellationToken);
        return filePath;
    }

    public Task<Stream?> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return Task.FromResult<Stream?>(null);

        Stream stream = File.OpenRead(filePath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return Task.FromResult(false);

        File.Delete(filePath);
        return Task.FromResult(true);
    }
}
