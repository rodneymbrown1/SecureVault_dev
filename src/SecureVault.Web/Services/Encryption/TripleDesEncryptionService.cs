using System.Security.Cryptography;
using SecureVault.Web.Data.Models;

namespace SecureVault.Web.Services.Encryption;

public class TripleDesEncryptionService : IEncryptionService
{
    private const int KeySizeBytes = 24; // 3DES uses 192-bit key
    private const int IVSizeBytes = 8;

    public EncryptionAlgorithm Algorithm => EncryptionAlgorithm.TripleDES;

    public async Task<EncryptionResult> EncryptAsync(Stream inputStream, CancellationToken cancellationToken = default)
    {
        using var tdes = TripleDES.Create();
        tdes.GenerateKey();
        tdes.GenerateIV();
        tdes.Mode = CipherMode.CBC;
        tdes.Padding = PaddingMode.PKCS7;

        using var encryptor = tdes.CreateEncryptor();
        using var msInput = new MemoryStream();
        await inputStream.CopyToAsync(msInput, cancellationToken);
        var plaintext = msInput.ToArray();

        var ciphertext = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

        // Prepend IV to ciphertext
        using var output = new MemoryStream();
        output.Write(tdes.IV);
        output.Write(ciphertext);

        // Compute HMAC for integrity
        using var hmac = new HMACSHA256(tdes.Key);
        var hmacValue = hmac.ComputeHash(output.ToArray());

        using var finalOutput = new MemoryStream();
        finalOutput.Write(output.ToArray());
        finalOutput.Write(hmacValue);

        return new EncryptionResult
        {
            EncryptedData = finalOutput.ToArray(),
            Key = tdes.Key,
            IV = tdes.IV,
            KeySize = KeySizeBytes * 8
        };
    }

    public async Task<DecryptionResult> DecryptAsync(Stream inputStream, byte[] key, byte[]? iv, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await inputStream.CopyToAsync(ms, cancellationToken);
        var data = ms.ToArray();

        // Verify HMAC (last 32 bytes)
        var hmacLength = 32;
        var payload = data.AsSpan(0, data.Length - hmacLength).ToArray();
        var receivedHmac = data.AsSpan(data.Length - hmacLength).ToArray();

        using var hmac = new HMACSHA256(key);
        var computedHmac = hmac.ComputeHash(payload);
        if (!CryptographicOperations.FixedTimeEquals(computedHmac, receivedHmac))
            throw new CryptographicException("HMAC verification failed. Data may have been tampered with.");

        // Parse: [IV(8)][ciphertext]
        var extractedIv = payload.AsSpan(0, IVSizeBytes).ToArray();
        var ciphertext = payload.AsSpan(IVSizeBytes).ToArray();

        using var tdes = TripleDES.Create();
        tdes.Key = key;
        tdes.IV = extractedIv;
        tdes.Mode = CipherMode.CBC;
        tdes.Padding = PaddingMode.PKCS7;

        using var decryptor = tdes.CreateDecryptor();
        var plaintext = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);

        return new DecryptionResult { DecryptedData = plaintext };
    }
}
