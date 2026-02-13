using SecureVault.Web.Data.Models;
using SecureVault.Web.Services.Encryption;

namespace SecureVault.Tests.Services;

public class DesEncryptionTests
{
    private readonly DesEncryptionService _service = new();

    [Fact]
    public void Algorithm_ReturnsDES()
    {
        Assert.Equal(EncryptionAlgorithm.DES, _service.Algorithm);
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip()
    {
        var plaintext = "Hello, DES encryption test!"u8.ToArray();
        using var inputStream = new MemoryStream(plaintext);

        var encryptResult = await _service.EncryptAsync(inputStream);

        Assert.NotNull(encryptResult.EncryptedData);
        Assert.NotNull(encryptResult.Key);
        Assert.Equal(64, encryptResult.KeySize);

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

        // Tamper with the encrypted data
        var tampered = encryptResult.EncryptedData.ToArray();
        tampered[10] ^= 0xFF;

        using var encryptedStream = new MemoryStream(tampered);
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _service.DecryptAsync(encryptedStream, encryptResult.Key, encryptResult.IV));
    }
}
