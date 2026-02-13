using SecureVault.Web.Data.Models;
using SecureVault.Web.Services.Encryption;

namespace SecureVault.Tests.Services;

public class TripleDesEncryptionTests
{
    private readonly TripleDesEncryptionService _service = new();

    [Fact]
    public void Algorithm_ReturnsTripleDES()
    {
        Assert.Equal(EncryptionAlgorithm.TripleDES, _service.Algorithm);
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip()
    {
        var plaintext = "Hello, TripleDES encryption test!"u8.ToArray();
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        Assert.NotNull(encryptResult.EncryptedData);
        Assert.NotNull(encryptResult.Key);
        Assert.Equal(192, encryptResult.KeySize);

        using var encryptedStream = new MemoryStream(encryptResult.EncryptedData);
        var decryptResult = await _service.DecryptAsync(encryptedStream, encryptResult.Key, encryptResult.IV);

        Assert.Equal(plaintext, decryptResult.DecryptedData);
    }

    [Fact]
    public async Task Decrypt_TamperedData_ThrowsException()
    {
        var plaintext = "test data"u8.ToArray();
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        var tampered = encryptResult.EncryptedData.ToArray();
        tampered[10] ^= 0xFF;

        using var encryptedStream = new MemoryStream(tampered);
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.DecryptAsync(encryptedStream, encryptResult.Key, encryptResult.IV));
    }
}
