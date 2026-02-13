using SecureVault.Web.Data.Models;
using SecureVault.Web.Services.Encryption;

namespace SecureVault.Tests.Services;

public class EccEncryptionTests
{
    private readonly EccEncryptionService _service = new();

    [Fact]
    public void Algorithm_ReturnsECC()
    {
        Assert.Equal(EncryptionAlgorithm.ECC, _service.Algorithm);
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip()
    {
        var plaintext = "Hello, ECC/ECDH hybrid encryption test!"u8.ToArray();
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        Assert.NotNull(encryptResult.EncryptedData);
        Assert.NotNull(encryptResult.Key);
        Assert.Equal(256, encryptResult.KeySize);
        Assert.Null(encryptResult.IV);

        using var encryptedStream = new MemoryStream(encryptResult.EncryptedData);
        var decryptResult = await _service.DecryptAsync(encryptedStream, encryptResult.Key, encryptResult.IV);

        Assert.Equal(plaintext, decryptResult.DecryptedData);
    }

    [Fact]
    public async Task EncryptDecrypt_LargeFile()
    {
        var plaintext = new byte[100 * 1024]; // 100KB
        Random.Shared.NextBytes(plaintext);
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        using var encryptedStream = new MemoryStream(encryptResult.EncryptedData);
        var decryptResult = await _service.DecryptAsync(encryptedStream, encryptResult.Key, encryptResult.IV);

        Assert.Equal(plaintext, decryptResult.DecryptedData);
    }

    [Fact]
    public async Task Encrypt_ProducesDifferentOutputEachTime()
    {
        var plaintext = "same input"u8.ToArray();

        using var stream1 = new MemoryStream(plaintext);
        var result1 = await _service.EncryptAsync(stream1);

        using var stream2 = new MemoryStream(plaintext);
        var result2 = await _service.EncryptAsync(stream2);

        // Different ephemeral keys each time
        Assert.NotEqual(result1.EncryptedData, result2.EncryptedData);
        Assert.NotEqual(result1.Key, result2.Key);
    }
}
