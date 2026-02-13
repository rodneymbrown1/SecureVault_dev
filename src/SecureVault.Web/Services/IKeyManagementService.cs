using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Services;

public interface IKeyManagementService
{
    Task<EncryptionKeyRecord> StoreKeyAsync(string userId, EncryptionAlgorithm algorithm, byte[] key, byte[]? iv, int keySize, CancellationToken cancellationToken = default);
    Task<(byte[] key, byte[]? iv)> RetrieveKeyAsync(Guid keyRecordId, string userId, CancellationToken cancellationToken = default);
    Task DeactivateKeyAsync(Guid keyRecordId, string userId, CancellationToken cancellationToken = default);
}
