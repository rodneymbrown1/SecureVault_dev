using System.Security.Cryptography;
using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Services.Encryption;

/// <summary>
/// Hybrid ECC encryption using ECDH key agreement + AES-256-GCM.
/// Generates an ephemeral ECDH key pair, derives a shared secret with the recipient's
/// public key, then encrypts data with AES-GCM using the derived key.
/// Output format: [ephemeralPubKeyLen(4)][ephemeralPubKey][nonce(12)][tag(16)][ciphertext]
/// The stored key is the ECDH private key, IV is not used at the key record level.
/// </summary>
public class EccEncryptionService : IEncryptionService
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;
    private const int DerivedKeySizeBytes = 32;

    public EncryptionAlgorithm Algorithm => EncryptionAlgorithm.ECC;

    public async Task<EncryptionResult> EncryptAsync(Stream inputStream, CancellationToken cancellationToken = default)
    {
        // Generate recipient key pair (stored) and ephemeral key pair (embedded in ciphertext)
        using var recipientKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        using var ephemeralKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

        // Derive shared secret from ephemeral private + recipient public
        var sharedSecret = ephemeralKey.DeriveKeyMaterial(recipientKey.PublicKey);

        // Derive AES key from shared secret using HKDF
        var aesKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, sharedSecret, DerivedKeySizeBytes, info: "SecureVault-ECC-AES"u8.ToArray());

        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);

        // Read plaintext
        using var msInput = new MemoryStream();
        await inputStream.CopyToAsync(msInput, cancellationToken);
        var plaintext = msInput.ToArray();

        // Encrypt with AES-GCM
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSizeBytes];
        using var aesGcm = new AesGcm(aesKey, TagSizeBytes);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        // Export ephemeral public key
        var ephemeralPubKey = ephemeralKey.PublicKey.ExportSubjectPublicKeyInfo();

        // Build output: [ephemeralPubKeyLen(4)][ephemeralPubKey][nonce(12)][tag(16)][ciphertext]
        using var output = new MemoryStream();
        output.Write(BitConverter.GetBytes(ephemeralPubKey.Length));
        output.Write(ephemeralPubKey);
        output.Write(nonce);
        output.Write(tag);
        output.Write(ciphertext);

        // Export recipient private key for storage (needed for decryption)
        var recipientPrivateKey = recipientKey.ExportPkcs8PrivateKey();

        return new EncryptionResult
        {
            EncryptedData = output.ToArray(),
            Key = recipientPrivateKey,
            IV = null,
            KeySize = 256 // P-256
        };
    }

    public async Task<DecryptionResult> DecryptAsync(Stream inputStream, byte[] key, byte[]? iv, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await inputStream.CopyToAsync(ms, cancellationToken);
        var data = ms.ToArray();

        // Import recipient private key
        using var recipientKey = ECDiffieHellman.Create();
        recipientKey.ImportPkcs8PrivateKey(key, out _);

        // Parse: [ephemeralPubKeyLen(4)][ephemeralPubKey][nonce(12)][tag(16)][ciphertext]
        var offset = 0;
        var ephemeralPubKeyLen = BitConverter.ToInt32(data, offset);
        offset += 4;

        var ephemeralPubKeyBytes = data.AsSpan(offset, ephemeralPubKeyLen).ToArray();
        offset += ephemeralPubKeyLen;

        var nonce = data.AsSpan(offset, NonceSizeBytes).ToArray();
        offset += NonceSizeBytes;

        var tag = data.AsSpan(offset, TagSizeBytes).ToArray();
        offset += TagSizeBytes;

        var ciphertext = data.AsSpan(offset).ToArray();

        // Import ephemeral public key and derive shared secret
        using var ephemeralPubKey = ECDiffieHellman.Create();
        ephemeralPubKey.ImportSubjectPublicKeyInfo(ephemeralPubKeyBytes, out _);

        var sharedSecret = recipientKey.DeriveKeyMaterial(ephemeralPubKey.PublicKey);

        // Derive AES key from shared secret
        var aesKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, sharedSecret, DerivedKeySizeBytes, info: "SecureVault-ECC-AES"u8.ToArray());

        // Decrypt with AES-GCM
        var plaintext = new byte[ciphertext.Length];
        using var aesGcm = new AesGcm(aesKey, TagSizeBytes);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return new DecryptionResult { DecryptedData = plaintext };
    }
}
