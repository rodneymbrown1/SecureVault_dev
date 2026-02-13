using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Services.Encryption;

public class EncryptionResult
{
    public byte[] EncryptedData { get; set; } = null!;
    public byte[] Key { get; set; } = null!;
    public byte[]? IV { get; set; }
    public byte[]? Tag { get; set; }
    public int KeySize { get; set; }
}

public class DecryptionResult
{
    public byte[] DecryptedData { get; set; } = null!;
}

public interface IEncryptionService
{
    EncryptionAlgorithm Algorithm { get; }
    Task<EncryptionResult> EncryptAsync(Stream inputStream, CancellationToken cancellationToken = default);
    Task<DecryptionResult> DecryptAsync(Stream inputStream, byte[] key, byte[]? iv, CancellationToken cancellationToken = default);
}
