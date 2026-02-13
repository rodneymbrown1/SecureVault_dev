using System.Security.Cryptography;
using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Services.Encryption;

public class AesEncryptionService : IEncryptionService
{
    private const int KeySizeBytes = 32; // AES-256
    private const int NonceSizeBytes = 12; // GCM nonce
    private const int TagSizeBytes = 16; // GCM tag

    public EncryptionAlgorithm Algorithm => EncryptionAlgorithm.AES;

    public async Task<EncryptionResult> EncryptAsync(Stream inputStream, CancellationToken cancellationToken = default)
    {
        var key = RandomNumberGenerator.GetBytes(KeySizeBytes);
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);

        using var ms = new MemoryStream();
        await inputStream.CopyToAsync(ms, cancellationToken);
        var plaintext = ms.ToArray();

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSizeBytes];

        using var aesGcm = new AesGcm(key, TagSizeBytes);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        // Output format: [nonce(12)][tag(16)][ciphertext]
        using var output = new MemoryStream();
        output.Write(nonce);
        output.Write(tag);
        output.Write(ciphertext);

        return new EncryptionResult
        {
            EncryptedData = output.ToArray(),
            Key = key,
            IV = nonce,
            Tag = tag,
            KeySize = KeySizeBytes * 8
        };
    }

    public async Task<DecryptionResult> DecryptAsync(Stream inputStream, byte[] key, byte[]? iv, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await inputStream.CopyToAsync(ms, cancellationToken);
        var data = ms.ToArray();

        // Parse format: [nonce(12)][tag(16)][ciphertext]
        var nonce = data.AsSpan(0, NonceSizeBytes).ToArray();
        var tag = data.AsSpan(NonceSizeBytes, TagSizeBytes).ToArray();
        var ciphertext = data.AsSpan(NonceSizeBytes + TagSizeBytes).ToArray();

        var plaintext = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(key, TagSizeBytes);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return new DecryptionResult { DecryptedData = plaintext };
    }
}
