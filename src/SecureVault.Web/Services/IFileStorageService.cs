namespace SecureVault.Web.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(string key, Stream data, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
