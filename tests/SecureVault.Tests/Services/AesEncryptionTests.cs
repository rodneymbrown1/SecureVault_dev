using SecureVault.Web.Data.Models;
using SecureVault.Web.Services.Encryption;

namespace SecureVault.Tests.Services;

public class AesEncryptionTests
{
    private readonly AesEncryptionService _service = new();

    [Fact]
    public void Algorithm_ReturnsAES()
    {
        Assert.Equal(EncryptionAlgorithm.AES, _service.Algorithm);
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip_SmallData()
    {
        var plaintext = "Hello, SecureVault!"u8.ToArray();
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        Assert.NotNull(encryptResult.EncryptedData);
        Assert.NotNull(encryptResult.Key);
        Assert.Equal(256, encryptResult.KeySize);
        Assert.NotEqual(plaintext, encryptResult.EncryptedData);

        using var encryptedStream = new MemoryStream(encryptResult.EncryptedData);
        var decryptResult = await _service.DecryptAsync(encryptedStream, encryptResult.Key, encryptResult.IV);

        Assert.Equal(plaintext, decryptResult.DecryptedData);
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip_LargeData()
    {
        var plaintext = new byte[1024 * 1024]; // 1MB
        Random.Shared.NextBytes(plaintext);
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        using var encryptedStream = new MemoryStream(encryptResult.EncryptedData);
        var decryptResult = await _service.DecryptAsync(encryptedStream, encryptResult.Key, encryptResult.IV);

        Assert.Equal(plaintext, decryptResult.DecryptedData);
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip_EmptyData()
    {
        var plaintext = Array.Empty<byte>();
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        using var encryptedStream = new MemoryStream(encryptResult.EncryptedData);
        var decryptResult = await _service.DecryptAsync(encryptedStream, encryptResult.Key, encryptResult.IV);

        Assert.Equal(plaintext, decryptResult.DecryptedData);
    }

    [Fact]
    public async Task Decrypt_WithWrongKey_ThrowsException()
    {
        var plaintext = "test data"u8.ToArray();
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        var wrongKey = new byte[32];
        Random.Shared.NextBytes(wrongKey);

        using var encryptedStream = new MemoryStream(encryptResult.EncryptedData);
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.DecryptAsync(encryptedStream, wrongKey, encryptResult.IV));
    }

    [Fact]
    public async Task Encrypt_ProducesDifferentOutputEachTime()
    {
        var plaintext = "same input"u8.ToArray();

        using var stream1 = new MemoryStream(plaintext);
        var result1 = await _service.EncryptAsync(stream1);

        using var stream2 = new MemoryStream(plaintext);
        var result2 = await _service.EncryptAsync(stream2);

        Assert.NotEqual(result1.EncryptedData, result2.EncryptedData);
        Assert.NotEqual(result1.Key, result2.Key);
    }
}
