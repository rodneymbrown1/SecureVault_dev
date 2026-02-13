using System.Security.Cryptography;
using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Services.Encryption;

/// <summary>
/// Hybrid RSA encryption: generates a random AES-256-GCM session key,
/// encrypts the file with AES-GCM, then wraps the session key with RSA-OAEP.
/// Output format: [RSA-encrypted-session-key-length(4)][RSA-encrypted-session-key][AES-GCM-nonce(12)][AES-GCM-tag(16)][ciphertext]
/// The stored key is the RSA private key (PKCS8), IV is not used at the key record level.
/// </summary>
public class RsaEncryptionService : IEncryptionService
{
    private const int RsaKeySizeBits = 2048;
    private const int AesKeySizeBytes = 32;
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    public EncryptionAlgorithm Algorithm => EncryptionAlgorithm.RSA;

    public async Task<EncryptionResult> EncryptAsync(Stream inputStream, CancellationToken cancellationToken = default)
    {
        // Generate RSA key pair
        using var rsa = RSA.Create(RsaKeySizeBits);

        // Generate random AES session key
        var aesKey = RandomNumberGenerator.GetBytes(AesKeySizeBytes);
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);

        // Read plaintext
        using var msInput = new MemoryStream();
        await inputStream.CopyToAsync(msInput, cancellationToken);
        var plaintext = msInput.ToArray();

        // Encrypt file data with AES-GCM
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSizeBytes];
        using var aesGcm = new AesGcm(aesKey, TagSizeBytes);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        // Wrap AES session key with RSA-OAEP
        var wrappedKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

        // Build output: [wrappedKeyLen(4)][wrappedKey][nonce(12)][tag(16)][ciphertext]
        using var output = new MemoryStream();
        output.Write(BitConverter.GetBytes(wrappedKey.Length));
        output.Write(wrappedKey);
        output.Write(nonce);
        output.Write(tag);
        output.Write(ciphertext);

        // Export RSA private key for storage
        var privateKey = rsa.ExportPkcs8PrivateKey();

        return new EncryptionResult
        {
            EncryptedData = output.ToArray(),
            Key = privateKey,
            IV = null, // RSA doesn't use IV at the key level
            KeySize = RsaKeySizeBits
        };
    }

    public async Task<DecryptionResult> DecryptAsync(Stream inputStream, byte[] key, byte[]? iv, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await inputStream.CopyToAsync(ms, cancellationToken);
        var data = ms.ToArray();

        // Import RSA private key
        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(key, out _);

        // Parse: [wrappedKeyLen(4)][wrappedKey][nonce(12)][tag(16)][ciphertext]
        var offset = 0;
        var wrappedKeyLen = BitConverter.ToInt32(data, offset);
        offset += 4;

        var wrappedKey = data.AsSpan(offset, wrappedKeyLen).ToArray();
        offset += wrappedKeyLen;

        var nonce = data.AsSpan(offset, NonceSizeBytes).ToArray();
        offset += NonceSizeBytes;

        var tag = data.AsSpan(offset, TagSizeBytes).ToArray();
        offset += TagSizeBytes;

        var ciphertext = data.AsSpan(offset).ToArray();

        // Unwrap AES session key
        var aesKey = rsa.Decrypt(wrappedKey, RSAEncryptionPadding.OaepSHA256);

        // Decrypt with AES-GCM
        var plaintext = new byte[ciphertext.Length];
        using var aesGcm = new AesGcm(aesKey, TagSizeBytes);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return new DecryptionResult { DecryptedData = plaintext };
    }
}
