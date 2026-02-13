using SecureVault.Web.Data.Models;
using SecureVault.Web.Services.Encryption;

namespace SecureVault.Tests.Services;

public class RsaEncryptionTests
{
    private readonly RsaEncryptionService _service = new();

    [Fact]
    public void Algorithm_ReturnsRSA()
    {
        Assert.Equal(EncryptionAlgorithm.RSA, _service.Algorithm);
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip()
    {
        var plaintext = "Hello, RSA hybrid encryption test!"u8.ToArray();
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        Assert.NotNull(encryptResult.EncryptedData);
        Assert.NotNull(encryptResult.Key);
        Assert.Equal(2048, encryptResult.KeySize);
        Assert.Null(encryptResult.IV); // RSA hybrid doesn't store IV at key level

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
}
